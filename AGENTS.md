# Project Instructions

## Business Requirements Reference

The official business requirements for this project are documented in:

docs/Elevate_Backend_User_Stories_by_Microservice.pdf

This document is the primary source of truth for:

- User Stories
- Acceptance Criteria
- Backend Subtasks
- Business Rules
- API requirements
- Microservice responsibilities

Whenever implementing a feature, identify the relevant SCRUM ID and read its complete User Story, Acceptance Criteria, and Backend Subtasks from this document before implementation.

Do not invent or assume business requirements that are not supported by the document.

## Technical Source of Truth

The existing repository is the source of truth for:

- Architecture
- Project structure
- Coding conventions
- Existing design patterns
- Existing abstractions
- Infrastructure
- Dependency Injection
- Authentication
- Database implementation

Do not replace existing project conventions with a different architecture.

## Feature Development Workflow

For every task:

1. Identify the SCRUM ID.
2. Locate the corresponding User Story in the PDF.
3. Read its Acceptance Criteria.
4. Read the related Backend Subtasks.
5. Inspect the existing implementation.
6. Reuse existing abstractions.
7. Follow the existing architecture.
8. Implement only the required scope.
9. Verify the implementation against every Acceptance Criterion.

## Architecture

The project uses Vertical Slice Architecture.

Each feature should own its endpoint/controller and feature-specific components inside its own Feature folder.

Do not create centralized controllers for unrelated features.

Follow the exact folder and naming conventions already present in the repository.

## Existing Patterns

Before creating a new abstraction, check whether the repository already provides:

- ASP.NET Identity
- UserManager
- MediatR
- Result Pattern
- AutoMapper
- FluentValidation
- JWT
- Refresh Tokens
- Serilog
- ApiResponse
- Repository Pattern
- Unit of Work

Reuse existing implementations whenever possible.

Do not duplicate existing functionality.

## Task Completion

After implementing a task, report:

- SCRUM ID
- User Story
- Acceptance Criteria verification
- Backend Subtasks verification
- Files created
- Files modified
- Assumptions
- Any requirements that could not be implemented
