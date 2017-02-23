
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


            string groupsClaimSourceIndex = (System.Web.Helpers.Json.Decode(claimsIdentity.FindFirst("_claim_names").Value)).groups;
            var groupClaimsSource = (System.Web.Helpers.Json.Decode(claimsIdentity.FindFirst("_claim_sources").Value))[groupsClaimSourceIndex];
            string requestUrl = groupClaimsSource.endpoint + "?api-version=1.6";

            string accesstoken = Request.Headers["X-MS-TOKEN-AAD-ID-TOKEN"].ToString();
            // Prepare and Make the POST request
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
            StringContent content = new StringContent("{\"securityEnabledOnly\": \"false\"}");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            request.Content = content;
            HttpResponseMessage response = await client.SendAsync(request);
            string responseContent = await response.Content.ReadAsStringAsync();


            // Endpoint returns JSON with an array of Group ObjectIDs
            if (response.IsSuccessStatusCode)
            {
                var groupsResult = (System.Web.Helpers.Json.Decode(responseContent)).value;

                foreach (string groupObjectID in groupsResult)
                    groupObjectIds.Add(groupObjectID);
            }
            else
            {
                groupObjectIds.Add("Response not success. It is " + response.StatusCode + " and Content = " + responseContent);
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
