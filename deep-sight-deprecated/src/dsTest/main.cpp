#include <iostream>

#include "core/Grid.h"
#include "core/Window.h"

void main()
{
	std::cout << "dsTest :: Basic test of DeepSight library" << std::endl;
	std::cout << "CITA (c) 2020" << std::endl;

	auto grid = DeepSight::FloatGrid::read("C:/git/deep-sight/test/data/p15_.vdb");
	auto res = DeepSight::TestOpenGLWindow();

	std::cout << "Exited with " << res << std::endl;

	system("pause");
}