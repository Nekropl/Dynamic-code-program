using System;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        string filename = null;
        public Form1()
        {
            InitializeComponent();
        }

        private string Browse()
        {
            // Выбор файла динамического кода
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (!string.IsNullOrWhiteSpace(dialog.FileName))
                {
                    // Проверяем расширение файла
                    if (Path.GetExtension(dialog.FileName).ToLower() == ".txt")
                    {
                        return dialog.FileName;
                    }
                    else
                    {
                        MessageBox.Show("Выбранный файл не является текстовым файлом (.txt)", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
                else
                {                  
                    MessageBox.Show("Не выбран файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private object DetermineDataType(string inputText)
        {
            //Определение типа переменной
            int intValue;
            if (int.TryParse(inputText, out intValue))
                return intValue;

            double doubleValue;
            if (double.TryParse(inputText, out doubleValue))
                return doubleValue;

            if (!string.IsNullOrEmpty(inputText))
                return inputText;

            return null;
        }

        private void CompileDynamicCode()
        {
            //Инициализация переменных
            object x = DetermineDataType(textBox1.Text);
            object y = DetermineDataType(textBox2.Text);

            if (filename == null)
            {
                MessageBox.Show("Выберете текстовый файл", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Загрузка кода из файла
            string code = File.ReadAllText(filename);

            // Создание компилятора
            CSharpCodeProvider provider = new CSharpCodeProvider();

            // Настройка параметров компиляции
            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;

            // Добавление ссылок на необходимые сборки
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            // Компиляция кода в сборку
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            // Проверка наличия ошибок при компиляции
            if (results.Errors.HasErrors)
            {
                Console.WriteLine("Ошибка при компиляции кода:");
                foreach (CompilerError error in results.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
            }
            else
            {
                // Получение типов из сборки
                Type[] types = results.CompiledAssembly.GetTypes();
                foreach (Type type in types)
                {
                    Type interfaceType = type.GetInterface("IDynamicCode");
                    if (interfaceType != null)
                    {
                        // Создание экземпляра классов наследников интерфейса IDynamicCode
                        object dynamicCode = Activator.CreateInstance(type);

                        // Получение метода из сборки
                        MethodInfo executeMethod = type.GetMethod("Execute");

                        // Передача параметров в метод
                        object[] methodArgs = new object[] { x, y };

                        // Вызов метода
                        executeMethod.Invoke(dynamicCode, methodArgs);
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CompileDynamicCode();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            // Запись пути файла в переменную
            filename = Browse();
        }
    }
}
