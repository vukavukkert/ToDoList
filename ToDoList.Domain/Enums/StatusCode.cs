namespace ToDoList.Domain.Enums;

public enum StatusCode
{
    OK = 200,
    InternalServerError = 500,
    TaskAlreadyExists = 1,
    TaskNotFound = 2,
}