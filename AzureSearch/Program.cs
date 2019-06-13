using System;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System.IO;
using System.Collections.Generic;

namespace AzureSearch
{
    class Program
    {
        private static readonly string searchServiceName = "gtplsearch";
        private static readonly string adminApiKey = "DE5FAD1F992B073D135A3477C45E3552";
        private static readonly string hotelFileName = "P:\\Practice Code\\Azure Search\\AzureSearch\\HotelData.txt";

        static void Main(string[] args)
        {
            var serviceClient = CreateSearchServiceClient();
            //CreateIndex(serviceClient);
            ImportData(serviceClient);
        }

        private static void ImportData(SearchServiceClient serviceClient)
        {
            var hotelsText = File.ReadAllLines(hotelFileName);
            var hotels = new List<Hotel>();
            for (int i = 1; i < hotelsText.Length; i++)
            {
                var hotelText = hotelsText[i];
                var hotelTextColumns = hotelText.Split("\t");
                hotels.Add(
                new Hotel()
                {
                    HotelId = hotelTextColumns[0],
                    HotelName = hotelTextColumns[1],
                    Description = hotelTextColumns[2],
                    DescriptionFr = hotelTextColumns[3],
                    Category = hotelTextColumns[4],
                    Tags = hotelTextColumns[5].Split(","),
                    ParkingIncluded = hotelTextColumns[6] == "0" ? false : true,
                    SmokingAllowed = hotelTextColumns[7] == "0" ? false : true,
                    LastRenovationDate = Convert.ToDateTime(hotelTextColumns[8]),
                    BaseRate = Convert.ToDouble(hotelTextColumns[9]),
                    Rating = (int)Convert.ToDouble(hotelTextColumns[10])
                });
            } // no error checking because demo code

            var actions = new List<IndexAction<Hotel>>();
            foreach (var hotel in hotels)
            {
                actions.Add(IndexAction.Upload(hotel));
            }

            var batch = IndexBatch.New(actions);

            try
            {
                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("hotels");
                indexClient.Documents.Index(batch);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void CreateIndex(SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = "hotels",
                Fields = FieldBuilder.BuildForType<Hotel>()
            };

            serviceClient.Indexes.Create(definition);
        }

        private static SearchServiceClient CreateSearchServiceClient()
        {
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }
    }
}
