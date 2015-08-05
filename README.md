CouchbaseItemsStats
===================

Reads a Couchbase's bucket metadata and generates a report with statistics about stored items.
The keys are grouped using a regex and for each group calculates:
* count
* average expiration time
* stardard deviation of expiration time

## Usage

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
2. Edit settings in config file
	* `<couchbase>`: Bucket connection.
	* `viewDesignName`: Design document/view name to consume.
    * `viewName`: View name to consume.
    * `keyPrefixRegex`: A regex used to extract the prefix from each key. It's used in grouping function.
    * `reportFormat`: Format used to store in the report. Only `json` is suppported by now.
    * `reportFilePath`: Path used to store the report file.
3. Run the .exe and the report file will be generated