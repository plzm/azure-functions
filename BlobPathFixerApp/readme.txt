This is an Azure function with blob input trigger.

This function takes the input blob, updates its path (in this case, by URL-decoding the path, replacing spaces with underscores, and prepending an output folder) to illustrate changing input to output blob path/name, which is not supported with standard Azure Functions bindings at this time.

The function uses the Azure Storage SDK to write the output blob.
