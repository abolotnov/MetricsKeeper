using System;
using Core.Context;
using AdysTech.InfluxDB.Client.Net;
using Microsoft.EntityFrameworkCore;
using Core.Data;
using Core.Tools;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace Core.Test
{
    public class DataInitializer
    {
        CoreContext _cont;
        CoreAuthContext _authCont;
        InfluxDBClient _influxClient;
        ConfigurationReader conf;

        public DataInitializer(IServiceProvider serviceProvider){
            conf = new ConfigurationReader();
            _cont = (CoreContext)serviceProvider.GetService(typeof(CoreContext));
            _authCont = (CoreAuthContext)serviceProvider.GetService(typeof(CoreAuthContext));
			_influxClient = new InfluxDBClient(conf.Configuration["Data:InfluxDBHost"],
										 conf.Configuration["Data:InfluxDBLogin"],
										 conf.Configuration["Data:InfluxDBPassword"]);
        }

        public void DeleteAllData(){
            string[] tables = {"Project", "Portfolio", "Org", "AspNetUsers", "OrgAccess",
                "AspNetUserTokens", "AspNetUserRoles", "AspNetUserLogins", "AspNetUserClaims", "AspNetRoleClaims", "AspNetUserClaims"};
            _cont.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=0");
            foreach (string table in tables){
                string command = String.Format("delete from {0}", table);
                _cont.Database.ExecuteSqlCommand(command);
            }
            _cont.Database.ExecuteSqlCommand("SET FOREIGN_KEY_CHECKS=1");
        }

        public void CreateOrgStructure(string path = "/Test/TestData.json"){
            string DataFilePath = AppContext.BaseDirectory + path;
            JObject json = JObject.Parse(File.ReadAllText(DataFilePath));
            foreach (JToken OrgToken in json["Orgs"])
			{
                Org o = OrgToken.ToObject<Org>();
				_cont.Orgs.Add(o);
                _cont.SaveChanges();
                _influxClient.CreateDatabaseAsync(o.MetricDatabaseName);
                foreach(JToken UserDataToken in OrgToken["Users"]){
                    CreateOrgUser(o.Id, UserDataToken.ToObject<OrgUserRegistrationData>());
                }
            }
        }

        public void CreateOrgUser(int OrgId, OrgUserRegistrationData regdata){
			ApplicationUser user = new ApplicationUser
			{
				UserName = regdata.Email,
				Email = regdata.Email,

			};
			var password = new PasswordHasher<ApplicationUser>();
			var hashedPassword = password.HashPassword(user, regdata.Password);
			user.PasswordHash = hashedPassword;

            var userStore = new UserStore<ApplicationUser>(_authCont)
            {
                AutoSaveChanges = true,
            };
            userStore.CreateAsync(user);

            OrgAccess NewOrgAccessor = new OrgAccess{
                OrganizationId = OrgId,
                AllowRead = regdata.AllowRead,
                AllowWrite = regdata.AllowWrite,
                AllowAdministration = regdata.IsOrgAdmin,
                ApplicationUserId = user.Id,
            };
			_cont.OrgAccess.Add(NewOrgAccessor);
			_cont.SaveChanges();
		}
    }
}
