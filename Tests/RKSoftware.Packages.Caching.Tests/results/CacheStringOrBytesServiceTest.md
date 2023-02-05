# SET

## SET (3000 items, json string length is 2515399, UTF8 byte array length is 2515399)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| + size, Mb                   | 9.3         | 6.8       |
| tested object, Mb            | 5           | 2.5       |
| the biggest byte array, Mb   | 4.2         | 4.2       |

## SET (100 items, json string length is 84082, UTF8 byte array length is 84082)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| + size, Mb                   | 0.61        | 0.53      |
| tested object, Mb            | 0.168       | 0.084     |
| the biggest byte array, Mb   | 0.131       | 0.131     |

## SET (10 items, json string length is 7816, UTF8 byte array length is 7816)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| + size, Mb                   | 0.23        | 0.24      |
| tested object, Mb            | 0.015       | 0.008     |
| the biggest byte array, Mb   | -           | -         |

# GET

## GET (3000 items, json string length is 2515399, UTF8 byte array length is 2515399)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| Redis result, Mb             | 2.5         | 4.2       |
| string, Mb                   | 5           | -         |
| tested object, Mb            | 3.3         | 3.3       |
| -----------------------------|------------ | ----------|
| diff, Mb                     | 11.461      | 8.291     |

## GET (100 items, json string length is 84082, UTF8 byte array length is 84082)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| Redis result, Mb             | 0.084       | 0.131     |
| string, Mb                   | 0.168       | -         |
| tested object, Mb            | 0.111       | 0.111     |
| -----------------------------|------------ | ----------|
| diff, Mb                     | 1.065       | 0.750     |

## GET (10 items, json string length is 7816, UTF8 byte array length is 7816)

|                              | String      | Bytes     |
| -----------------------------|------------ | ----------|
| Redis result, Mb             | 0.08        | 0.08      |
| string, Mb                   | 0.016       | -         |
| tested object, Mb            | 0.01        | 0.01      |
| -----------------------------|------------ | ----------|
| diff, Mb                     | 0.423       | 0.402     |