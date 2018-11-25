using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UI_Test
{
    class Program
    {
        public delegate object ExecuteFunction(MethodInfo method, object[] inputs);

        static void Main(string[] args)
        {
            string s1 = typeof(double).FullName;
            string s2 = typeof(Double).FullName;

            MethodInfo method = typeof(Program).GetMethod("DoStuff");

            var delType = Expression.GetDelegateType(method.GetParameters().Select(x => x.ParameterType).Concat( new Type[] { method.ReturnType }).ToArray());
            builtFunc = Delegate.CreateDelegate(delType, method);

            ParameterExpression lambdaInput = Expression.Parameter(typeof(object[]), "x");
            Expression[] inputs = method.GetParameters().Select((x, i) => Expression.Convert(Expression.ArrayIndex(lambdaInput, Expression.Constant(i)), x.ParameterType)).ToArray();
            var methodExpression = Expression.Call(method, inputs);
            
            compiledFunc = Expression.Lambda<Func<object[], object>>(methodExpression, lambdaInput).Compile();

            List<double> ratios = TestSpeed(new List<ExecuteFunction> { Execute0, Execute1, Execute2, Execute3, Execute4, Execute5, Execute6 });
        }

        private static List<double> TestSpeed(List<ExecuteFunction> functions, int testTime = 2000)
        {
            MethodInfo method = typeof(Program).GetMethod("DoStuff");

            object[] inputs =
            {
                "test",
                new CustomObject { CustomData = { { "A", 1 }, { "B", 2 } } },
                5,
                3.6
            };

            Stopwatch stopwatch = new Stopwatch();
            long seed = System.Environment.TickCount; 	// Prevents the JIT Compiler 
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(2); // Use only the second core 
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            //Warm up 
            Console.WriteLine("Warming up...");
            int count = 0;
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < 1500)  // A Warmup of 1000-1500 mS stabilizes the CPU cache and pipeline.                
                WarmUpFunction(seed, count++); // Warmup
            stopwatch.Stop();

            //Direct Invoke
            List<int> counts = new List<int>();
            count = 0;
            Console.Write("Test Method DoStuff... ");
            stopwatch.Reset();
            stopwatch.Start();
            while (stopwatch.ElapsedMilliseconds < testTime)
            {
                DoStuff(inputs[0] as string, inputs[1] as CustomObject, (int)inputs[2], (double)inputs[3]);
                count++;
            }
            stopwatch.Stop();
            counts.Add(count);
            Console.Write(count.ToString() + " iteration\n");

            // Test the other options
            foreach (ExecuteFunction function in functions)
            {
                count = 0;
                Console.Write("Test Method " + function.Method.Name + "... ");
                stopwatch.Reset();
                stopwatch.Start();
                while (stopwatch.ElapsedMilliseconds < testTime)
                {
                    function(method, inputs);
                    count++;
                }
                stopwatch.Stop();
                counts.Add(count);
                Console.Write(count.ToString() + " iteration\n");
            }

            int maxIndex = 0;
            double maxCount = 0;
            for (int i = 0; i < counts.Count; i++)
            {
                if (counts[i] > maxCount)
                {
                    maxCount = counts[i];
                    maxIndex = i;
                }
            }

            List<double> ratios = counts.Select(x => ((double)x) / maxCount).ToList();

            for (int i = 0; i < functions.Count; i++)
            {
                Console.WriteLine("Count ratio: " + ratios[i+1].ToString("N3") + " -> \t Method " + functions[i].Method.Name);
            }

            return ratios;
        }

        private static object Execute0(MethodInfo method, object[] inputs)
        {
            return DoStuff(
                inputs[0] as dynamic,
                inputs[1] as dynamic,
                inputs[2] as dynamic,
                inputs[3] as dynamic
           );
        }

        private static object Execute1(MethodInfo method, object[] inputs)
        {
            return method.Invoke(null, inputs);
        }

        private static object Execute2(MethodInfo method, object[] inputs)
        {
            switch (inputs.Length)
            {
                case 4:
                    return CallMethod(method, 
                        inputs[0] as dynamic,
                        inputs[1] as dynamic,
                        inputs[2] as dynamic,
                        inputs[3] as dynamic,
                        Activator.CreateInstance(method.ReturnType) as dynamic
                   );
                default:
                    return null;
            }
        }

        private static object Execute3(MethodInfo method, object[] inputs)
        {
            return definedFunc(inputs[0] as dynamic, inputs[1] as dynamic, inputs[2] as dynamic, inputs[3] as dynamic);
        }

        private static object Execute4(MethodInfo method, object[] inputs)
        {
            switch (inputs.Length)
            {
                case 4:
                    return CallMethod2(method,
                        inputs[0] as dynamic,
                        inputs[1] as dynamic,
                        inputs[2] as dynamic,
                        inputs[3] as dynamic,
                        Activator.CreateInstance(method.ReturnType) as dynamic
                   );
                default:
                    return null;
            }
        }

        private static object Execute5(MethodInfo method, object[] inputs)
        {
            return builtFunc(inputs[0] as dynamic, inputs[1] as dynamic, inputs[2] as dynamic, inputs[3] as dynamic);
        }

        private static object Execute6(MethodInfo method, object[] inputs)
        {
            return compiledFunc(inputs);
        }


        public static object CallMethod<T1, T2, T3, T4, T5>(MethodInfo method, T1 d1, T2 d2, T3 d3, T4 d4, T5 d5)
        {
            Func<T1, T2, T3, T4, T5> func = (Func<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5>), method);
            return func(d1, d2, d3, d4);
        }

        public static object CallMethod2<T1, T2, T3, T4, T5>(MethodInfo method, T1 d1, T2 d2, T3 d3, T4 d4, T5 d5)
        {
            if (dynamicFunc == null)
                dynamicFunc = (Func<T1, T2, T3, T4, T5>)Delegate.CreateDelegate(typeof(Func<T1, T2, T3, T4, T5>), method);
            return dynamicFunc(d1, d2, d3, d4);
        }

        public static CustomObject DoStuff(string s, CustomObject obj, int val1, double val2)
        {
            return new CustomObject { CustomData = { { "A", s.Length + obj.CustomData.Count + val1 + val2 } } };
        }


        static long WarmUpFunction(long seed, int count)
        {
            long result = seed;
            for (int i = 0; i < count; ++i)
            {
                result ^= i ^ seed; // Some useless bit operations
            }
            return result;
        }

        public static dynamic dynamicFunc = null;
        public static dynamic builtFunc = null;
        public static Func<string, CustomObject, int, double, CustomObject> definedFunc = (Func<string, CustomObject, int, double, CustomObject>)Delegate.CreateDelegate(typeof(Func<string, CustomObject, int, double, CustomObject>), typeof(Program).GetMethod("DoStuff"));
        public static Func<object[], object> compiledFunc = null;
    }
}
