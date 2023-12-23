# StuPro - student project matching

A command line program that uses the HiGHs solver
to solve the following optimization problem:

There is a set of students and a set of project. 
Each student has rated each project. 
Assign each student to one project maximizing the over all satisfaction.

## Prerequisites
- Install the [HiGHs solver](https://highs.dev/). On Mac with Homebrew just run `brew install highs`
- Download and extract the latest version of [Stupro](https://github.com/xithan/stupro/releases)

## Usage

Run the programm with a path to a csv file:
```
stupro ratings.csv 
```

The csv file is expected to have a column "Name" with the students' names
and for each project a column with the project's name as header and a rating for each student.

Example:

| Name      |Project 1|Project 2|Project 3|
|-----------| ---- | ---- | ---- |
| Student A |1|2|6|
| Student B |4|3|3|
| Student C |4|3|3|


Further configurations can be done in the config.yml configuration file or you can pass
a path to a different yml file (`stupro input.csv -config=my_config.yml`)

See [example_config.yml](https://github.com/xithan/stupro/blob/main/example_config.yml) for further details.

The program can also generate test data.  
Run the program with -h to get further informations about the parameters.
