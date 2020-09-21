# AmmoScraper
Webscraper aimed at finding in stock ammo

<b>Note: this was a project to learn web scraping in C#, they will blacklist your IP.</b>

This was a personal project aimed at tracking in stock ammo at TargetSportsUSA.com
Mostly thrown together for fun and best practices and design patterms were not aimed for.

Specific items can be searched for and further refined with omission terms.
An example search in json:
```
[
  {
    "URL": "https://www.targetsportsusa.com/45-acp-auto-ammo-c-70.aspx",
    "SearchTerms": [
      "230",
      "fmj",
      "full metal jacket"
    ],
    "OmitTerms": [
      "jhp",
      "hollow"
    ],
    "InStockProducts": [],
    "MatchedProducts": []
  }
]
```
The scraper would then send email notifications using Gmail to a specific email address using a Google app password. 
The email configuration file is generated on the first run and puts placeholder information for the user to enter.
