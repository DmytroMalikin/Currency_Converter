﻿using CurrencyConverter.Api.Json;
using CurrencyConverter.Data.Models.DataModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace CurrencyConverter.Api
{
    public class DollarApi
    {
        private string _WebUrl = @"http://www.mycurrency.net/service/rates";

        public object JsonModelUnils { get; private set; }

        private string GetData()
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                using (HttpClient client = new HttpClient(handler, false))
                {
                    var responseFromServer = client.GetAsync(_WebUrl).Result.Content.ReadAsStringAsync().Result;

                    return responseFromServer;
                }
            }
        }
        
        public List<Asset> GetAssets()
        {
            var assets = JsonModelUtils.AssetsDeserializeFromUsd(GetData()).ToList();

            assets.Add(new Asset
            {
                AssetId = "USD",
                Name = "United States dollar",
                IsTypeCrypto = false
            });

            return assets;
        }

        public ExchangeRate GetUsdRate(Asset asset)
        {
            return JsonModelUtils.ExchangeRateDeserializeFromUsd(asset, GetData());           
        }
    }
}
