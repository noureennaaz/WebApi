
# Entity Management API

## API Endpoints:

### Create Entity (POST api/Values/create-entity)
Creates a new entity with the provided data.

**Input Format:** 
```json
{
  "addresses": [
    {
      "addressLine": "string",
      "city": "string",
      "country": "string"
    }
  ],
  "dates": [
    {
      "dateType": "string",
      "dateOnly": "2024-03-08T13:44:10.007Z"
    }
  ],
  "deceased": true,
  "gender": "string",
  "names": [
    {
      "firstName": "string",
      "middleName": "string",
      "surname": "string"
    }
  ]
}
```


### Get Entity By Id (GET get-entity-by-id/{id})
Retrieves the entity with the specified ID.

**Input Format -**
```json
{
    "id":"string"
}
```
**Output Format:** 
```json
{
  "id": "65ec3b997c3d7271297c67da",
  "addresses": [
    {
      "addressLine": "Times Square",
      "city": "NYC",
      "country": "USA"
    }
  ],
  "dates": [
    {
      "dateType": "registered on:",
      "dateOnly": "2024-03-09T10:36:09.45Z"
    },
    {
      "dateType": "DOB",
      "dateOnly": "1990-03-07T18:30:00Z"
    }
  ],
  "deceased": false,
  "gender": "string",
  "names": [
    {
      "firstName": "John",
      "middleName": "",
      "surname": "Doe"
    }
  ]
}

```

### Search Entities (GET /search):
Retrieves a list of entities with the search string matching the following fields-

-  Address Country
-  Address Line
-  FirstName
-  MiddleName
-  Surname

**Input Format -**
```json
{
   "search":"string"
}
```
**Optional URL parameters -**
- **pageSize** : with maximum supported value 10. Default value is 10
- **page** : default value is 1 
**Output Format -**
```json
{
  "data": [
    {
      "id": "string",
      "addresses": [
        {
          "addressLine": "Times Square",
          "city": "NYC",
          "country": "USA"
        }
      ],
      "dates": [
        {
          "dateType": "registered on:",
          "dateOnly": "2024-03-09T10:37:37.699Z"
        },
        {
          "dateType": "DOB",
          "dateOnly": "2004-03-08T18:30:00Z"
        }
      ],
      "deceased": false,
      "gender": "string",
      "names": [
        {
          "firstName": "Dave",
          "middleName": "",
          "surname": "Grey"
        }
      ]
    }
  ],
  "totalPages": 8,
  "pageNumber": 1
}

```

### Search Entities Based on Parameters (GET /filter):
Retrieves a list of entities with optional search and filter parameters.
Can search based on these parameters.

**Input Format -**
```json
{
  "gender": "male",
  "startDate": "2024-01-01",
  "endDate": "2024-03-01",
  "countries": "USA, Canada"
}
```
**Optional URL parameters -**
- **pageSize** : with maximum supported value 10. Default value is 10
- **page** : default value is 1 

**Output Format -**
```json
{
  "data": [
    {
      "id": "65ec3bf17c3d7271297c67db",
      "addresses": [
        {
          "addressLine": "Times Square",
          "city": "NYC",
          "country": "USA"
        }
      ],
      "dates": [
        {
          "dateType": "registered on:",
          "dateOnly": "2024-03-09T10:37:37.699Z"
        },
        {
          "dateType": "DOB",
          "dateOnly": "2004-03-08T18:30:00Z"
        }
      ],
      "deceased": false,
      "gender": "string",
      "names": [
        {
          "firstName": "Dave",
          "middleName": "",
          "surname": "Grey"
        }
      ]
    }
  ],
  "totalPages": 8,
  "pageNumber": 1
}

```

### List Entities (GET /api/get-all-entities):

Retrieves all the entities within the database.

**Optional URL parameters -**
- **pageSize** : with maximum supported value 10. Default value is 10
- **page** : default value is 1 

**Output Format -**
```json
{
  "data": [
    {
      "id": "65ec3bf17c3d7271297c67db",
      "addresses": [
        {
          "addressLine": "Times Square",
          "city": "NYC",
          "country": "USA"
        }
      ],
      "dates": [
        {
          "dateType": "registered on:",
          "dateOnly": "2024-03-09T10:37:37.699Z"
        },
        {
          "dateType": "DOB",
          "dateOnly": "2004-03-08T18:30:00Z"
        }
      ],
      "deceased": false,
      "gender": "string",
      "names": [
        {
          "firstName": "Dave",
          "middleName": "",
          "surname": "Grey"
        }
      ]
    }
  ],
  "totalPages": 8,
  "pageNumber": 1
}

```


### Update Entity (POST /api/update):

Updates an existing entity with the provided data.

**Input Format -** 
```json
{
  "id": "string",
  "addresses": [
    {
      "addressLine": "string",
      "city": "string",
      "country": "string"
    }
  ],
  "dates": [
    {
      "dateType": "string",
      "dateOnly": "2024-01-08"
    }
  ],
  "deceased": true,
  "gender": "string",
  "names": [
    {
      "firstName": "string",
      "middleName": "string",
      "surname": "string"
    }
  ]
}
```

### Delete Entity (DELETE /api/entities/{id}):

Deletes the entity with the specified ID.

**Input Format -**
```json

{
   "id":"string"
}
```

## Technology Stack:

- **ASP.NET Core**: Backend framework for building web APIs.
- **MongoDB**: NoSQL database for storing entity data.
- **Swagger**: API documentation tool for describing and visualizing API endpoints.
- **C#**: Primary programming language for backend development.
- **RESTful API**: Adheres to RESTful principles for designing web APIs.


