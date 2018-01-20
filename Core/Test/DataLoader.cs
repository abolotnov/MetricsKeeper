using System;
using Core.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.IO;
using Core.Context;
using Microsoft.EntityFrameworkCore;
using Core.Tools;
using Microsoft.Extensions.Logging;
using AdysTech.InfluxDB.Client.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace Core.Test
{
    public class DataLoader
    {
        CoreContext _cont;
        //CoreAuthContext _authCont;
        readonly InfluxDBClient _influxClient;
        //readonly ILogger _logger;
        public DataLoader(bool CleanDB = true)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CoreContext>();
            optionsBuilder.UseMySql(new ConfigurationReader().Configuration["Data:MySQLDBConnectionString"]);
            _cont = new CoreContext(optionsBuilder.Options);

            //var authOptionsBuilder = new DbContextOptionsBuilder<CoreAuthContext>();
            //authOptionsBuilder.UseMySql(new ConfigurationReader().Configuration["Data:MySQLDBConnectionString"]);
            //_authCont = new CoreAuthContext(authOptionsBuilder.Options);

            if (CleanDB == true){
                WipeDBRecords();
            }
			ConfigurationReader conf = new ConfigurationReader();
            //default configuration of InfluxDB ignores login and password parameters unless specifically configured
            _influxClient = new InfluxDBClient(conf.Configuration["Data:InfluxDBHost"],
										 conf.Configuration["Data:InfluxDBLogin"],
										 conf.Configuration["Data:InfluxDBPassword"]);

        }

        private void WipeDBRecords(){
            var ProjDelRes = _cont.Database.ExecuteSqlCommand("DELETE FROM PROJECT where ID < 0");
            var PortDelRes = _cont.Database.ExecuteSqlCommand("DELETE FROM PORTFOLIO where ID < 0");
            var OrgDelRes = _cont.Database.ExecuteSqlCommand("DELETE FROM ORG where ID < 0");
			/*
            var userStore = new UserStore<ApplicationUser>(_authCont);
            userStore.AutoSaveChanges = true;
            List<ApplicationUser> allusers = userStore.Users.ToListAsync().Result;
            foreach(var u in allusers){
                userStore.DeleteAsync(u);
            }
            */


			//todo: figure out a way to drop all influx databases
			//current client lib does not provide this functionality nor statements execution vehicle
			//maybe create CQ for drop db and then drop CQ?
		}

        public void LoadData()
        {
            JObject json = JObject.Parse(File.ReadAllText(AppContext.BaseDirectory+"/Test/TestData.json"));
            foreach(JToken OToken in json["Orgs"].Children()){
                Org o = OToken.ToObject<Org>();
                _cont.Orgs.Add(o);
                _cont.SaveChanges();
                _influxClient.CreateDatabaseAsync(o.MetricDatabaseName);

                /*
                foreach (JToken userToken in OToken["Users"])
                {
                    ApplicationUserRegistration UserRegData = userToken.ToObject<ApplicationUserRegistration>();
                    ApplicationUser user = new ApplicationUser
                    {
                        UserName = UserRegData.Email,
                        Email = UserRegData.Email,

                    };
                    var password = new PasswordHasher<ApplicationUser>();
                    var hashedPassword = password.HashPassword(user, UserRegData.Password);
                    user.PasswordHash = hashedPassword;

                    var userStore = new UserStore<ApplicationUser>(_authCont);
                    userStore.AutoSaveChanges = true;

                    userStore.CreateAsync(user);
                    OrgAccess oa = new OrgAccess();
                    oa.OrganizationId = o.Id;
                    oa.AllowRead = true;
                    oa.AllowWrite = UserRegData.IsOrgAdmin;
                    oa.AllowAdministration = UserRegData.IsOrgAdmin;

                    _cont.OrgAccess.Add(oa);
                    _cont.SaveChanges();
                    if (user.OrganizationsAccess == null){
                        user.OrganizationsAccess = new List<OrgAccess>();
                    }
                    user.OrganizationsAccess.Add(oa);
                    userStore.UpdateAsync(user);
                }
                */
                foreach(JToken prtToken in OToken["Portfolios"]){
                    Portfolio p = prtToken.ToObject<Portfolio>();
                    p.OrganizationId = o.Id;
                    _cont.Portfolios.Add(p);
                    _cont.SaveChanges();
                    foreach(JToken projToken in prtToken["Projects"]){
                        Project prj = projToken.ToObject<Project>();
                        prj.PortfolioId = p.Id;
                        _cont.Projects.Add(prj);
                        _cont.SaveChanges();
                    }
                }
            }

        }
    }
}
