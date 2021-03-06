﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CurrencyConverter.Api.Interfaces;
using CurrencyConverter.Data;
using CurrencyConverter.Data.Models.DataModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace CurrencyConverter.Api.Jobs
{
    public class AssetsUpdateJob : BackgroundJobProcess
    {
        private IDbContext _dbContext;

        private ICurrencyApi _currencyApi;

        private Dictionary<string, AssetData> _currencyCodeNames;

        public AssetsUpdateJob(IDbContext dbContext, ICurrencyApi currencyApi)
        {
            _dbContext = dbContext;

            _currencyApi = currencyApi;

            LoadCurrenciesNames();
        }

        public override void Execute()
        {
            var dbAssets = _dbContext.Set<Asset>();

            var apiAssets = _currencyApi.GetAssets()
                .GroupBy(x => new { x.AssetId, x.IsTypeCrypto })
                .Select(x => x.First());

            foreach (var apiAsset in apiAssets)
            {
                var dbAsset = dbAssets.FirstOrDefault(x => x.AssetId == apiAsset.AssetId 
                    && x.IsTypeCrypto == apiAsset.IsTypeCrypto);

                if (dbAsset == null)
                {
                    if (!apiAsset.IsTypeCrypto && _currencyCodeNames.ContainsKey(apiAsset.AssetId))
                    {
                        apiAsset.Name = _currencyCodeNames[apiAsset.AssetId].Name;
                    }

                    dbAssets.Add(
                        new Asset
                        {
                            AssetId = apiAsset.AssetId,
                            Name = apiAsset.Name,
                            IsTypeCrypto = apiAsset.IsTypeCrypto
                        });
                }
                else
                {
                    dbAsset.Name = apiAsset.Name;
                    dbAsset.IsTypeCrypto = apiAsset.IsTypeCrypto;
                }
            }

            _dbContext.SaveChanges();      
        }

        private void LoadCurrenciesNames()
        {
            _currencyCodeNames = new Dictionary<string, AssetData>();

            string jsonCurrencyData;

            using (StreamReader r = new StreamReader(@"D:\Programming\CurrencyProject\CurrencyConverter\CurrencyConverter.Api\currencies.json"))
            {
                jsonCurrencyData = r.ReadToEnd();
            }

            JObject currenciesObject = JObject.Parse(jsonCurrencyData);

            foreach (JProperty prop in currenciesObject.Properties())
            {
                var asset = prop.Value;

                _currencyCodeNames.Add(prop.Name,
                    new AssetData
                    {
                        Name = asset["name"].ToString(),
                        Symbol = asset["symbol"].ToString(),
                        SymbolNavive = asset["symbol_native"].ToString()
                    });
            }
        }
    }

    internal class AssetData
    {
        public string Name { get; set; }

        public string Symbol { get; set; }

        public string SymbolNavive { get; set; }
    }
}