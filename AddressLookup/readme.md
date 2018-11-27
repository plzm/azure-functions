Address lookup from address API(s). An Azure Function that is triggered by new/updated documents in the Cosmos DB document change feed, and for each changed input, writes an output to a different collection. The output contains the input data plus location/address lookup data from the Bing Maps API.

Initial implementation against Bing Maps API using the Bing Maps REST Toolkit, see https://github.com/Microsoft/BingMapsRESTToolkit.

Please note: to use, you should set valid values for application settings CosmosDBConnection and BingMapsAPIKey.

---

# PLEASE NOTE FOR THE ENTIRETY OF THIS REPOSITORY AND ALL ASSETS
## 1. No warranties or guarantees are made or implied.
## 2. All assets here are provided by me "as is". Use at your own risk. Validate before use.
## 3. I am not representing my employer with these assets, and my employer assumes no liability whatsoever, and will not provide support, for any use of these assets.
## 4. Use of the assets in this repo in your Azure environment may or will incur Azure usage and charges. You are completely responsible for monitoring and managing your Azure and other usage usage.

---

Unless otherwise noted, all assets here are authored by me. Feel free to examine, learn from, comment, and re-use (subject to the above) as needed and without intellectual property restrictions.

If anything here helps you, attribution and/or a quick note is much appreciated.