﻿namespace uLearn.Courses.BasicProgramming.Slides.U09_Sorting
{
	[Slide("Практика", "{A2864473-5DCB-46D7-A43D-96F48A40C2CE}")]
	public class S099_Exercise
	{
		/* 

		Autocomplete (1 балл за каждую задачу)
		---

		[Скачать проект autocomplete](autocomplete.zip)

		Во многих программах в разных контекстах можно увидеть функцию автодополнения вводимого текста.
		Автодополнение обычно работает так: есть какой-то готовый словарь, содержащий все значения, 
		которые автодополнение может подсказывать, а когда пользователь вводит начало некоторого слова — ему 
		показывают несколько подходящих слов из словаря, начинающиеся с букв, уже введенных пользователем.

		Такую функцию очень просто реализовать "в лоб", если словарь небольшой. 
		Если же словарь большой, то необходимо задумываться об эффективности алгоритма.

		* Запустите проект autocomplete и поизучайте программу. В частности попробуйте набрать ZZZZZ — поиск будет работать крайне медленно.
		* В проекте autocomplete выполните все задания, написанные в комментариях в файле Autocompleter.cs

		Обязательным требованием является наличие модульных тестов, проверяющих корректность работы поиска.

		
		Root (1 балл)
		----

		Вам нужно написать консольную программу, которая получает на вход данные из параметров командной строки (массив args в функции Main).
		Программа должна найти корни полинома с некоторой точностью с заданными коэффициентами на указанном отрезке.

		Ваша программа должна запускаться следующим образом:

		```
		root.exe <left> <right> <a0> <a1> <a2> ... <aN>
		```

		* Если `f(left)` и `f(right)` имеют одинаковый знак, то программа должна выйти сообщив об этом, без поиска решения.
		* Она должна выводить решение уравнения `f(x) = 0`, где `f(x) = a0 + a1*x + ... + aN*x^N` на отрезке `left..right`, с точностью 1e-6, 
		то есть выведенное значение должно отличаться от истинного значения корня не более чем на 1e-6.
		
		Обязательным требованием является наличие модульных тестов, проверяющих корректность работы алгоритма поиска корня.
		 
		*/
	}
}