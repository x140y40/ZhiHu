﻿using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using System.Diagnostics;

namespace Common
{
    public   class UserManage
    {
        private string html;
        private string url_token;
      
        public UserManage(string urltoken)
         {
             url_token = urltoken;
      
         }    
        private void  GetUserInformation(string json)
        {  
                JObject obj = JObject.Parse(json);
                string xpath = "['" + url_token + "']";
                JToken tocken = obj.SelectToken("['entities']").SelectToken("['users']").SelectToken(xpath);
                RedisCore.PushIntoList(2, "User", tocken.ToString());
              
        }  
        private void  GetUserFlowerandNext(string json)
        {
                 string foollowed = "https://www.zhihu.com/api/v4/members/" + url_token + "/followers?include=data%5B*%5D.answer_count%2Carticles_count%2Cfollower_count%2Cis_followed%2Cis_following%2Cbadge%5B%3F(type%3Dbest_answerer)%5D.topics&offset=0&limit=20";
                 string following = "https://www.zhihu.com/api/v4/members/" + url_token + "/followees?include=data%5B%2A%5D.answer_count%2Carticles_count%2Cfollower_count%2Cis_followed%2Cis_following%2Cbadge%5B%3F%28type%3Dbest_answerer%29%5D.topics&limit=20&offset=0";
                 RedisCore.PushIntoList(1, "nexturl", following);
                 RedisCore.PushIntoList(1, "nexturl", foollowed);
           
        }
        private bool GetHtml()
        {                 
            string url="https://www.zhihu.com/people/"+url_token+"/following";
            html = HttpHelp.DownLoadString(url);
            return  !string.IsNullOrEmpty(html);
        }
        public  void  analyse()
        {
                if (GetHtml())
                {
                    try
                    {
                        Stopwatch watch = new Stopwatch();
                        watch.Start();
                        HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(html);
                        HtmlNode node = doc.GetElementbyId("data");
                        StringBuilder stringbuilder =new StringBuilder(node.GetAttributeValue("data-state", ""));
                        stringbuilder.Replace("&quot;", "'");           
                        stringbuilder.Replace("&lt;", "<");
                        stringbuilder.Replace("&gt;", ">");
                        GetUserInformation(stringbuilder.ToString());
                        GetUserFlowerandNext(stringbuilder.ToString());
                        watch.Stop();
                        Console.WriteLine("分析Html用了{0}毫秒", watch.ElapsedMilliseconds.ToString());
                       
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            
            }    
        }
    
}
