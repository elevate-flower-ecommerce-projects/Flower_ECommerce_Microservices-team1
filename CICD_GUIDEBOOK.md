# 📘 Flower E-Commerce Microservices (Team 1) - CI/CD Pipeline Guidebook

Welcome to the official **CI/CD Pipeline Guidebook** for Team 1's Flower E-Commerce Microservices project.

---

## 📑 Table of Contents
1. [Chapter 1: Overview & Architecture](#chapter-1-overview--architecture)
2. [Chapter 2: Prerequisites & Secrets Setup (Docker Hub & GitHub)](#chapter-2-prerequisites--secrets-setup-docker-hub--github)
3. [Chapter 3: Deep Dive into the CI/CD Workflow File](#chapter-3-deep-dive-into-the-cicd-workflow-file)

---

## Chapter 1: Overview & Architecture

### Purpose
The CI/CD pipeline automates code quality checks, container image compilation, and artifact publishing for Team 1's platform. Every time code is pushed to `main`, `master`, or `Development`, GitHub Actions automatically:
- Builds container images for all Team 1 microservices in parallel using Buildx.
- Pushes Docker images to **Docker Hub** (`amr0110`) using matrix tag names.

### Team 1 Microservices Map

| Service Name | Dockerfile Location | Docker Hub Image Name |
| :--- | :--- | :--- |
| **API Gateway** | `./API Gateway/Dockerfile` | `flower-apigateway-team1` |
| **Identity Service** | `./Identity service/Dockerfile` | `flower-identity-service-team1` |
| **Address & Store Coverage** | `./Address & Store Coverage Service/Dockerfile` | `flower-address-service-team1` |
| **Cart Service** | `./Cart Service/Dockerfile` | `flower-cart-service-team1` |
| **Catalog Service** | `./Catalog Service/Dockerfile` | `flower-catalog-service-team1` |
| **Order & Fulfillment Service** | `./Order & Fulfillment Service/Dockerfile` | `flower-order-service-team1` |
| **Payment Service** | `./Payment Service/Dockerfile` | `flower-payment-service-team1` |

---

## Chapter 2: Prerequisites & Secrets Setup (Docker Hub & GitHub)

1. Sign up on [hub.docker.com](https://hub.docker.com).
2. Generate a Personal Access Token (PAT) under **Account Settings > Personal Access Tokens**.
3. Add repository secrets in GitHub (**Settings > Secrets and variables > Actions**):
   - `DOCKER_USERNAME`: `amr0110`
   - `DOCKER_PASSWORD`: `<YOUR_DOCKER_HUB_PERSONAL_ACCESS_TOKEN>`

---

## Chapter 3: Deep Dive into the CI/CD Workflow File

Workflow configuration file: [.github/workflows/ci-cd.yml](file:///d:/partition%20h/Elevate/flower%20ecommerce/team1/Flower_ECommerce_Microservices-team1/.github/workflows/ci-cd.yml)
Triggers on pushes and PRs to `main`, `master`, and `Development` branches.
