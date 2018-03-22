using Microsoft.Azure.CognitiveServices.Search.EntitySearch;
using Microsoft.Azure.CognitiveServices.Search.EntitySearch.Models;
using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Services
{
    public static class BingEntitySearch
    {

        private static EntitySearchAPI entitySearchAPI = new EntitySearchAPI(new
                ApiKeyServiceClientCredentials("628767743b5b4967a539705afbee6870"));
        //8ffc62fb0c0745d9934f24916d776046
        public static Person SearchForPerson(string personName)
        {


            var entityData = entitySearchAPI.Entities.Search(query: personName);
            Debug.Write(entityData);
            if (entityData?.Entities?.Value?.Count > 0)
            {
                Thing mainEntity = entityData.Entities.Value.Where(thing => thing.EntityPresentationInfo.EntityScenario == EntityScenario.DominantEntity).FirstOrDefault();

                if (mainEntity != null)
                {
                    Debug.WriteLine($"response=== {mainEntity}");
                    return new Person { Name = mainEntity.Name, Description = mainEntity.Description, ImageUrl = mainEntity.Image.ThumbnailUrl };

                }
            }

            return null;
        }
    }
}