# Contributing to MyIslandGame

Thank you for considering contributing to MyIslandGame! This document outlines the process for contributing to the project.

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone.

## How Can I Contribute?

### Reporting Bugs

- Check if the bug has already been reported in the Issues
- Use the bug report template when creating a new issue
- Include detailed steps to reproduce the problem
- Include information about your environment (OS, .NET version, etc.)

### Suggesting Features

- Check if the feature has already been suggested in the Issues
- Use the feature request template when creating a new issue
- Be specific about what the feature should do
- Consider how the feature fits into the overall vision of the game

### Pull Requests

1. Fork the repository
2. Create a new branch for your changes
3. Implement your changes
4. Ensure your code follows the project's coding style
5. Add or update tests as necessary
6. Update documentation to reflect your changes
7. Create a pull request using the template

## Development Workflow

### Setting Up the Development Environment

1. Install the .NET 8.0 SDK
2. Install MonoGame
3. Clone the repository
4. Run `dotnet build` to build the project
5. Run `dotnet run` to run the game

### Coding Guidelines

- Follow C# naming conventions
- Use XML documentation comments for public classes and methods
- Keep methods small and focused on a single responsibility
- Use meaningful variable and method names
- Follow the existing architectural patterns (especially ECS)

### Commit Messages

- Use clear, descriptive commit messages
- Start with a verb in the present tense (e.g., "Add", "Fix", "Update")
- Reference issue numbers when relevant

### Testing

- Add appropriate tests for new features
- Ensure all tests pass before submitting a pull request
- Manual testing is also important for gameplay-related changes

## Project Structure

- `/Source` - Main game code
  - `/ECS` - Entity Component System
    - `/Components` - Component classes
    - `/Systems` - System classes
  - `/States` - Game state management
  - `/Input` - Input handling
  - `/Rendering` - Rendering-related code
- `/Content` - Game assets
- `/Tests` - Test projects

## Getting Help

If you have questions about contributing, feel free to:
- Open an issue with your question
- Contact the project maintainers

Thank you for contributing to MyIslandGame!
