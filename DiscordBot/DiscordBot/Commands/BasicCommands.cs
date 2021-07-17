using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Commands
{
    public enum JsonElementType
    {
        GetUsdString,
        GetEurString
    }

    class BasicCommands : BaseCommandModule
    {
        CancellationTokenSource cts;
        static HttpClient client = new HttpClient();
        private TimeSpan delayTimer = new TimeSpan(0, 0, 0, 3);

        [Command("Ping")]
        [Description("checks if the Ether bot and used API's are online")]
        public async Task PrintMessage(CommandContext ctx)
        {
            await ctx.Member.SendMessageAsync("Bot is online!").ConfigureAwait(false);

            HttpResponseMessage response;

            response = await client.GetAsync("https://api.etherscan.io/api?module=stats&action=ethprice&apikey=F9X7UBBP6EAEDUGTVGXTQCQJ8BNRF35MIU");
            if (response.IsSuccessStatusCode)
            {
                await ctx.Member.SendMessageAsync("Ehterscan API is online!").ConfigureAwait(false);
            }

            response = await client.GetAsync("https://free.currconv.com/api/v7/convert?q=USD_EUR&compact=ultra&apiKey=c24220137a0957c3cb16");
            if (response.IsSuccessStatusCode)
            {
                await ctx.Member.SendMessageAsync("Currconv API is online!").ConfigureAwait(false);
            }
        }

        [Command("getether")]
        [Description("checks current ETH prices in USDollars (or euro's, this is an optional parameter!).")]
        public async Task GetEther(CommandContext ctx, string eur = null)
        {
            HttpResponseMessage response = await client.GetAsync("https://api.etherscan.io/api?module=stats&action=ethprice&apikey=F9X7UBBP6EAEDUGTVGXTQCQJ8BNRF35MIU");
            if (response.IsSuccessStatusCode)
            {
                string etherPriceInString = GetJsonElementData(response, JsonElementType.GetUsdString);
                if (etherPriceInString != string.Empty)
                {
                    if (eur == "euro")
                    {
                        response = await client.GetAsync("https://free.currconv.com/api/v7/convert?q=USD_EUR&compact=ultra&apiKey=c24220137a0957c3cb16");
                        float finalPrice = float.Parse(GetJsonElementData(response, JsonElementType.GetEurString)) * float.Parse(etherPriceInString);
                        await ctx.Member.SendMessageAsync("===EURO===").ConfigureAwait(false);
                        await ctx.Member.SendMessageAsync(finalPrice.ToString()).ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.Member.SendMessageAsync("===DOLLAR===").ConfigureAwait(false);
                        await ctx.Member.SendMessageAsync(etherPriceInString.ToString()).ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.Member.SendMessageAsync("Could not get prices on ETH, connection to API established but without the proper response!").ConfigureAwait(false);
                }
            }
            else
            {
                await ctx.Member.SendMessageAsync("Connection Failure").ConfigureAwait(false);
            }
        }

        [Command("conethercheck")] //short for continuous ethereum check
        [Description("Continuous checking of ether prices in USDollars (or euro's, this is an optional parameter!).")]
        public async Task DoContinuousCheck(CommandContext ctx, string eur = null)
        {
            cts = new CancellationTokenSource();

            await ctx.Member.SendMessageAsync("Continuous price check of ether initiated!").ConfigureAwait(false);

            await GetEther(ctx, eur);

            while (!cts.IsCancellationRequested)
            {
                await GetEther(ctx, eur);
                await Task.Delay(delayTimer);
            }
        }

        [Command("cancelcheck")]
        [Description("Stops continuous checking of ETH prices.")]
        public async Task CancelContinuousCheck(CommandContext ctx, string eur = null)
        {
            if (!cts.IsCancellationRequested)
            {
                await ctx.Member.SendMessageAsync("Cancelling continuous checking of ether!").ConfigureAwait(false);
                cts.Cancel();
            }
            else
            {
                await ctx.Member.SendMessageAsync("Nothing to cancel!").ConfigureAwait(false);
            }
        }

        private string GetJsonElementData(HttpResponseMessage response, JsonElementType type)
        {
            switch (type)
            {
                case JsonElementType.GetUsdString:
                    string USDobj = response.Content.ReadAsStringAsync().Result;
                    JObject USDuserObj = JObject.Parse(USDobj);
                    string USDelementString = Convert.ToString(USDuserObj["result"]["ethusd"]);
                    return USDelementString;
                case JsonElementType.GetEurString:
                    string EURobj = response.Content.ReadAsStringAsync().Result;
                    JObject EURuserObj = JObject.Parse(EURobj);
                    string EURelementString = Convert.ToString(EURuserObj["USD_EUR"]);
                    return EURelementString;
            }
            return "No switch case value inserted";
        }
    }
}