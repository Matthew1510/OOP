using LogiCore.App.Console;

System.Console.WriteLine("Добро пожаловать в LogiCore!");
ConsoleApplication application = new ConsoleApplication();
string? commandFilePath = null;
if (args.Length > 0)
    commandFilePath = args[0];
application.Run(commandFilePath);
// точка входа