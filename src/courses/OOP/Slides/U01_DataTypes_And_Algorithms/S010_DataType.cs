﻿using System;
using uLearn;

namespace OOP.Slides.U01_DataTypes_And_Algorithms
{
	[Slide("Класс как тип данных", "{96ACF39F-1D42-430A-AADA-A31FA6FED09E}")]
	public class S010_DataType
	{
		/*
		Самое простое и естественное применение классов — это создание новых типов данных по аналогии с `DateTime`, `String`, `TimeSpan`.

		В качестве примера, давайте создадим тип данных для вектора в двумерном пространстве.
	
		*/

		public class Vector
		{
			// Конструктор — это способ создать новый экземпляр этого типа данных.
			public Vector(double x, double y)
			{
				X = x;
				Y = y;
			}

			// У вектора есть две координаты:
			public double X { get; private set; }
			public double Y { get; private set; }

			[HideOnSlide]
			public Vector Subtract(Vector v)
			{
				return new Vector(X - v.X, Y - v.Y);
			}

			[HideOnSlide]
			public double Len()
			{
				return Math.Sqrt(X*X + Y*Y);
			}
		}

		/*
		В такой класс очень удобно помещать все функции по работе с векторами.
		В программе, делающей геометрические расчеты придется множество раз складывать вектора, вычитать, умножать, брать длинну. 
		Все это несложные операции и их выделение в отдельные функции может показаться чрезмерной декомпозицией. 
		Однако, в данном случае перенос этих функций в класс `Vector` заметно повысит читаемость использующего их кода.
		
		Например, вот выражение для вычисления расстояния между двумя точками написанное двумя способами: через координаты, и с использованием вспомогательных методов вычитания и вычисления длины.
		*/
		[ShowBodyOnSlide]
		public void LengthDemo(Vector from, Vector to)
		{
			var len1 = Math.Sqrt((to.X - from.X) * (to.X - from.X) + (from.Y - to.Y) * (to.Y - from.Y));
			
			var len2 = to.Subtract(from).Len();
		}

		/*
		В первом случае от букв рябит в глазах, и как следствие, очень просто пропустить в такой строчке какую-нибудь ошибку, не так ли?
		И действительно, заметили ли вы намеренно допущенную в первой строке ошибку? :-)

		Вторая строка предельно проста: вычесть, взять длину. Возможностей для незамеченной ошибки гораздо-гораздо меньше.
		
		Таким образом, однажды написанные вспомогательные методы упрощают код работы с векторами везде в вашей программе. 
		
		### Вывод

		Всегда помещайте методы манипуляции с типом данных в класс этого типа данных. Это избавит клиентский код от дублирования, сделает клиентский код проще и уменьшит вероятность случайных ошибок.
		*/
	}
}