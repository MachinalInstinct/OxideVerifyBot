using System.Text;
using System.Text.RegularExpressions;
using System;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;
using System.Linq;

namespace Oxide.Plugins
{
    [Info("DiscordLink", "MachinalInstinct", "1.0.2")]
    internal class MysqlTest : RustPlugin
    {
		private readonly Core.MySql.Libraries.MySql _mySql = Interface.Oxide.GetLibrary<Core.MySql.Libraries.MySql>();
        private Core.Database.Connection _mySqlConnection;
		private const string SelectData = "SELECT * FROM discordbot.discord_link";
		public static string UpdateSqlq = "UPDATE discordbot.discord_link SET SteamID=";
		[ChatCommand("link")]
        private void LinkQuery(BasePlayer player, string command, string[] args)
        {
			if (args.Length == 0)
			{
				player.ChatMessage(Syntax);
				return;
			}
			
            _mySqlConnection = _mySql.OpenDb("mysql.server.ip", 3306, "mysql.server.db", "mysql.db.user", "mysql.db.user.pw", this);

			var Authkey = args[0];
			Authkey = Authkey.Substring(0, Math.Min(5, Authkey.Length));
			string RegPattern = ("[/'\\~#%&*{}/:<>?|\"-]");
			string RegReplace = "";
			Regex filt = new Regex(RegPattern);
			Authkey = Regex.Replace(filt.Replace(Authkey, RegReplace),@"\s+", " ");
			var SteamID = player.userID;
			var sqls = Core.Database.Sql.Builder.Append(SelectData+" WHERE SteamID='"+SteamID+"' LIMIT 1;");
            _mySql.Query(sqls, _mySqlConnection, steamlist =>
            {
				if (steamlist.Count <= 0)
				{
				var sql = Core.Database.Sql.Builder.Append(SelectData+" WHERE AuthKey = '"+Authkey+"' LIMIT 1;");
				_mySql.Query(sql, _mySqlConnection, list =>
					{
					if (list.Count <= 0)
					{
						player.ChatMessage(AuthKeyWrongMessage); 
					}
					foreach (var entry in list)
						{
							if (list == null) return;
							{
								var AuthKeyServer = entry["AuthKey"].ToString();
								var VerifiedStatus = entry["Verified"].ToString();
								var USqlQ = Core.Database.Sql.Builder.Append(UpdateSqlq+"'"+SteamID+"', Verified='1' WHERE AuthKey='"+AuthKeyServer+"' LIMIT 1;");
								if(VerifiedStatus == "0")
								{	
									player.ChatMessage(AuthMessage+" "+AuthKeyServer+"\nSuccessful!"); 
									_mySql.Update(USqlQ, _mySqlConnection);
								}
								else 
								{ 
									player.ChatMessage(AlreadyAuthMessage); 
								}
							}			
						}				
				
					});	
				}
				else 
				{ 
					player.ChatMessage(AlreadyAuthMessage); 
				}
			});			
		}
	    	string Syntax = "Missing Authentication Key.\nHave you already Inititated linking on discord using .link?";
		string AuthKeyWrongMessage = "Tried linking your Steam account to Discord but failed.\nWrong Link Key!";
		string AuthMessage = "Linking your Steam account to Discord with key:";
		string AlreadyAuthMessage = "You have already linked your accounts.";
	}
}
