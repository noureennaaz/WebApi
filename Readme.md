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

### List Entities (GET /api/get-all-entities):

Retrieves all the entities within the database.



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
