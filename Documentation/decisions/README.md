# Decisions
Decision should be recorded and put in the decisions folder. Decisions should also be added to the index of this file.

## Index

- [Three-Layer Project Architecture for Cross-Platform CI/CD](three-layer-architecture.md) - Project structure enabling fast, cost-effective Linux-based testing
- [Vertical Slice Architecture with Parallel Folders](vertical-slice-architecture.md) - Feature organization across layers maintaining cohesion and discoverability
- [ContentView Over ContentPage for Features](contentview-over-contentpage.md) - UI component pattern for better composability and reusability
- [Separation of Stateful Services/ViewModels and Pure Function Libraries](pure-function-operations.md) - Pattern for organizing business logic to maximize testability and maintainability


## Template
Decisions should follow this template:
```
# [Title of problem]
Brief summary explaining the problem

# Decision
We've chosen [name of the solution], because
...

## Impact
Describe the impact of this decision

# Solution proposals
## [Name of solution proposal]
Brief description of solution proposal
Optional link to a thorough description of the solution proposal - a document found in decisions/details

### Pros/cons
- list of pros and cons of this solution proposal
```