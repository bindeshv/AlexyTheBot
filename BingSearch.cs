using Microsoft.Azure.CognitiveServices.Search.EntitySearch;
using Microsoft.Azure.CognitiveServices.Search.EntitySearch.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SimpleEchoBot
{
    public static class BingSearch
    {

        private static EntitySearchAPI entitySearchAPI = new EntitySearchAPI(new
                ApiKeyServiceClientCredentials("97ffb5f584bc499d8dffada8f97ed45f"));
        //8ffc62fb0c0745d9934f24916d776046
        public static string SearchForPerson(string personName)
        {


            var entityData = entitySearchAPI.Entities.Search(query: personName);
            Debug.Write(entityData);
            if (entityData?.Entities?.Value?.Count > 0)
            {
                var mainEntity = entityData.Entities.Value.Where(thing => thing.EntityPresentationInfo.EntityScenario == EntityScenario.DominantEntity).FirstOrDefault();

                if (mainEntity != null)
                {
                    Debug.WriteLine($"response=== {mainEntity}");
                    return mainEntity.Description;

                }
            }

            return null;
        }
    }
}