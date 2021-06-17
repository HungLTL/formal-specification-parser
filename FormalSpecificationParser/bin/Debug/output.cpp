#include <iostream>
using namespace std;

void Input(float &score) {
	cout << "Input float variable score: ";
	cin >> score;
}

bool Check_RateStudent(float score) {
	if ( ((score>=0)&&(score<=10))
)
		return true;
	else
		return false;
}

string RateStudent(float score) {
	string rating;
	if (score>=9)
		 rating="Excellent";
	if ((score>=8) && (score<9))
		rating="Good";
	if ((score>=6.5) && (score<8))
		rating="OK";
	if ((score>=5) && (score<6.5))
		rating="Average";
	if ((score>=3.5) && (score<5))
		rating="Bad";
	if (score<3.5)
		rating="Terrible";
	return rating;
}

int main() {
	float score = 0;
	string rating;

	do {
		Input(score);
		if (!Check_RateStudent(score))
			cout << "One or more parameters have an invalid input. Please retry.\n";
	} while (!Check_RateStudent(score));

	rating = RateStudent(score);
	cout << "Result: " << rating;
	system("pause");
	return 0;
}