
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http;
using System.Configuration;
using System.Net.Http.Headers;
using System.Web.Helpers;
using AwesomeWebApp.Models;
using System.Net;
using System.IO;

namespace AwesomeWebApp.Controllers
{
    public class EasyAuthController : Controller
    {
        // GET: EasyAuth
        public async Task<ActionResult> Index()
        {
           

            List<string> objectIdsToCompare = new List<string>();
            objectIdsToCompare = await GetMemberGroups(ClaimsPrincipal.Current.Identity as ClaimsIdentity);


            Group g = new Group();
            g.Names = objectIdsToCompare;
            return View(g);
        }



        async Task<List<string>> GetGroupsFromGraphAPI(ClaimsIdentity claimsIdentity)
        {
            List<string> groupObjectIds = new List<string>();


            //string groupsClaimSourceIndex = (System.Web.Helpers.Json.Decode(claimsIdentity.FindFirst("_claim_names").Value)).groups;
            //var groupClaimsSource = (System.Web.Helpers.Json.Decode(claimsIdentity.FindFirst("_claim_sources").Value))[groupsClaimSourceIndex];
            //string requestUrl = groupClaimsSource.endpoint + "?api-version=1.6";

            //string accesstoken = Request.Headers["X-MS-TOKEN-AAD-ACCESS-TOKEN"].ToString();
            //// Prepare and Make the POST request
            //HttpClient client = new HttpClient();
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
            //StringContent content = new StringContent("{\"securityEnabledOnly\": \"false\"}");
            //content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //request.Content = content;
            //HttpResponseMessage response = await client.SendAsync(request);
            //string responseContent = await response.Content.ReadAsStringAsync();


            //// Endpoint returns JSON with an array of Group ObjectIDs
            //if (response.IsSuccessStatusCode)
            //{
            //    var groupsResult = (System.Web.Helpers.Json.Decode(responseContent)).value;

            //    foreach (string groupObjectID in groupsResult)
            //        groupObjectIds.Add(groupObjectID);
            //}
            //else
            //{
            //    groupObjectIds.Add("Response not success. It is " + response.StatusCode + " and Content = " + responseContent);
            //}

            string accessToken = this.Request.Headers["X-MS-TOKEN-AAD-ACCESS-TOKEN"];

            // Call into the Azure AD Graph API using HTTP primitives and the
            // Azure AD access token.
            var url = "https://graph.windows.net/me/thumbnailPhoto?api-version=1.6";
            var request = WebRequest.CreateHttp(url);
            var headerValue = "Bearer " + accessToken;
            request.Headers.Add(HttpRequestHeader.Authorization, headerValue);

            using (var response = await request.GetResponseAsync())
            using (var responseStream = response.GetResponseStream())
            using (var memoryStream = new MemoryStream())
            {
                responseStream.CopyTo(memoryStream);
                string encodedImage = Convert.ToBase64String(
                  memoryStream.ToArray());

                groupObjectIds.Add("image = " + encodedImage);

                // do something with encodedImage, like embed it into your HTML...
            }

            return groupObjectIds;
        }


        async Task<List<string>> GetMemberGroups(ClaimsIdentity claimsIdentity)
        {
            //check for groups overage claim. If present query graph API for group membership
            if (claimsIdentity.FindFirst("_claim_names") != null
                && (System.Web.Helpers.Json.Decode(claimsIdentity.FindFirst("_claim_names").Value)).groups != null)
                return await GetGroupsFromGraphAPI(claimsIdentity);

            return claimsIdentity.FindAll("groups").Select(c => c.Value).ToList();
        }

    }
}
