#!/bin/bash

migration_name=$1

update_sqlite() {
    echo "Updating SQLite database..."
    dotnet ef database update --context SqliteConsoleDbContext
}

update_mssql() {
    echo "Updating MSSQL database..."
    dotnet ef database update --context MssqlConsoleDbContext
}

add_migrations() {
    echo "Adding migrations for SQLite and MSSQL databases with name '${migration_name}'..."
    export ef_migrate=1; dotnet ef migrations add "${migration_name}" --context SqliteConsoleDbContext -o Migrations/Sqlite/
    export ef_migrate=1; dotnet ef migrations add "${migration_name}" --context MssqlConsoleDbContext -o Migrations/Mssql/
}

print_menu() {
    echo "------------------------------------"
    echo "Which database do you want to update?"
    echo "1. SQLite"
    echo "2. MSSQL"
    echo "3. Both"
    echo "4. None"
    echo "------------------------------------"
}

validate_choice() {
    if [[ ! $1 =~ ^[1-4]$ ]]; then
        echo "Invalid choice. Please try again."
        return 1
    fi
    return 0
}

main() {
    if [ -z "$migration_name" ]; then
        read -p "Please enter the name of the migration: " migration_name
    fi

    add_migrations

    while true; do
        print_menu
        read -p "Enter your choice (1-4): " choice

        if validate_choice $choice; then
            break
        fi
    done

    case $choice in
        1)
            update_sqlite
            ;;
        2)
            update_mssql
            ;;
        3)
            update_sqlite
            update_mssql
            ;;
        4)
            echo "No database update will be performed."
            ;;
    esac
}

main