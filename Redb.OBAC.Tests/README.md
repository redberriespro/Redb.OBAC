# manual test run: preparation steps
- set up mongodb, mysql, postgres, mssql (all backends perfectly runs in containers)
- copy appsettings.example.json to appsettings.json
- make sure "Copy to output directory" set for appsettings.json 
- edit configuration strings for each backend
- run the tests