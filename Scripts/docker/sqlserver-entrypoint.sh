#!/bin/bash
# Start SQL Server in the background, wait until it's ready, then run the schema script.

# Start SQL Server
/opt/mssql/bin/sqlservr &
MSSQL_PID=$!

echo "Waiting for SQL Server to start..."
for i in $(seq 1 30); do
	/opt/mssql-tools18/bin/sqlcmd \
		-S localhost -U sa -P "CrudSamples!Pass1" \
		-Q "SELECT 1" -b -C > /dev/null 2>&1
	if [ $? -eq 0 ]; then
		echo "SQL Server is up."
		break
	fi
	echo "  attempt $i/30 - waiting 2s..."
	sleep 2
done

echo "Creating database and running schema script..."
/opt/mssql-tools18/bin/sqlcmd \
	-S localhost -U sa -P "CrudSamples!Pass1" -C \
	-Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CrudDatastoreTest') CREATE DATABASE CrudDatastoreTest;"

/opt/mssql-tools18/bin/sqlcmd \
	-S localhost -U sa -P "CrudSamples!Pass1" -C \
	-d CrudDatastoreTest \
	-i /scripts/SqlServer.sql

echo "Schema initialised."

# Hand off to the SQL Server process
wait $MSSQL_PID
