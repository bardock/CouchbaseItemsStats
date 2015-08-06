CouchbaseItemsStats
===================

Reads a Couchbase's bucket metadata and generates a report with statistics about stored items.
The keys are grouped using a regex and for each group calculates:
* count
* average seconds left for expiration
* stardard deviation of seconds left for expiration

Expired items are discriminated because [couchbase removes expired items at maintenance intervals](http://docs.couchbase.com/developer/dev-guide-3.0/keys-values.html).

## Lib usage

1. Add a view in the bucket with the following code:
	* Go to Couchbase Console (Web UI) > `Views` > `Create Development View`
	* Enter a `Design Document Name` and `View Name`
	* Enter the following code:
	```js
    function (doc, meta) {
		emit(meta.id, meta);
    }
    ```
    * Save
2. Add config section
	```xml
    <configSections>
    	<section name="couchbaseItemsStats" type="CouchbaseItemsStats.ConfigSection, CouchbaseItemsStats" />
    </configSections>
    
    <couchbaseItemsStats
        viewDesignName="dev_Keys"
        viewName="Metadata"
        keyPrefixRegex="^(CacheContext_)?[^_]+" />
	```
	* `viewDesignName`: Design document/view name to consume.
    * `viewName`: View name to consume.
    * `keyPrefixRegex`: A regex used to extract the prefix from each key. It's used in grouping function.
3. Obtain the stats:
	```csharp
    var couchbaseClient = new CouchbaseClient();
    var client = new StatsClient(couchbaseClient);
    var stats = client.GetStats();
	```

## Console app usage
1. Edit settings in config file
	* `<couchbase>`: Bucket connection.
	* `<couchbaseItemsStats>`: Lib config (see previous section).
    * `reportFormat`: Format used to store in the report. Only `json` is suppported by now.
    * `reportFilePath`: File path used to store the report.
2. Run the .exe and the report file will be generated