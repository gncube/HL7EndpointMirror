# HL7-Endpoint-Mirror — Product Requirements Document (PRD)

## 1. Executive Summary

**Elevator Pitch:**  
HL7-Endpoint-Mirror is a serverless Azure Function utility that emulates a healthcare provider’s HL7 ingestion endpoint. It enables the Genomics Lab System and related teams to perform safe, realistic end-to-end integration, load, and chaos testing—without touching live clinical systems.

**Problem Statement:**  
Testing HL7 integrations against real clinical endpoints is risky, slow, and often impossible due to privacy and operational constraints. Teams need a safe, observable, and configurable mock endpoint to validate message delivery, error handling, and system resilience.

**Solution Overview:**  
Deploy a resilient, observable HL7 endpoint mock on Azure Functions, supporting ACK generation, chaos/failure simulation, and full telemetry, to accelerate development and de-risk production releases.

---

## 2. Target Audience

- **Primary Users:**
  - Backend engineers (integration, reliability, and load testing)
  - QA/SDETs (data integrity and protocol validation)
  - DevOps (latency, connectivity, and monitoring)

- **User Personas:**
  - _Integration Engineer_: Needs to verify HL7 message delivery and error handling.
  - _QA Analyst_: Validates that outbound HL7 messages are correctly formatted and acknowledged.
  - _DevOps Engineer_: Monitors system health, latency, and outbound connectivity.

- **Market Size:**
  - Internal use for Genomics Lab System and partner teams; extensible for other healthcare integration projects.

---

## 3. Product Scope

- **In Scope:**
  - Accept HL7 v2 messages via HTTPS POST
  - Generate HL7 ACKs with correct correlation
  - Simulate failures and latency (chaos mode)
  - Structured logging and Application Insights telemetry
  - Azure Function key authentication

- **Out of Scope:**
  - Storing or forwarding HL7 messages to real clinical systems
  - Supporting HL7 v3 or FHIR
  - UI for configuration (env vars only for v1)

- **Future Considerations:**
  - Admin dashboard for live configuration
  - Support for additional HL7 versions or protocols
  - Integration with other test harnesses

---

## 4. Functional Requirements

- **Core Features:**
  - POST `/api/v1/hl7/messages` endpoint (HTTPS, `application/hl7-v2`)
  - Extract `X-Message-Id` header for correlation
  - Parse MSH segment to extract `MessageControlId`
  - Generate HL7 ACK (MSA segment) with original `MessageControlId`
  - Chaos mode: configurable failure rate, error type, and latency

- **Secondary Features:**
  - Latency simulation (optional, via env var)
  - Customizable error codes/statuses

- **Integration Requirements:**
  - Azure Application Insights for telemetry
  - Azure Function Level Keys for authentication

---

## 5. Non-Functional Requirements

- **Performance:**
  - ACK response time < 200ms (excluding cold starts)
  - Scalable to handle high-volume load tests

- **Security:**
  - Enforce TLS 1.2+
  - Require `x-functions-key` for all requests

- **Reliability:**
  - 99.9% uptime (in line with Azure Functions SLA)
  - All requests logged with correlation ID and status

- **Usability:**
  - Simple, well-documented API
  - Accessible logs and metrics via Application Insights

---

## 6. User Experience

- **User Journey:**
  1. Engineer configures endpoint URL and function key in test system.
  2. System sends HL7 message to `/api/v1/hl7/messages` with `X-Message-Id`.
  3. Mirror logs request, simulates chaos if enabled, and returns HL7 ACK.
  4. Engineer reviews logs and telemetry in Application Insights.

- **Key User Stories:**
  1. As a backend engineer, I want to send HL7 messages and receive valid ACKs so that I can verify integration logic.
  2. As a QA analyst, I want to simulate endpoint failures so that I can test error handling and retry logic.
  3. As a DevOps engineer, I want to monitor request latency and error rates so that I can ensure system health.
  4. As a tester, I want to correlate requests and responses using `X-Message-Id` so that I can trace transactions.
  5. As a developer, I want to configure chaos mode via environment variables so that I can automate resilience tests.
  6. As a security lead, I want all traffic to require authentication and TLS so that test data is protected.

- **UI/UX Guidelines:**
  - No UI; API-only.
  - Clear API documentation and sample requests/responses.

---

## 7. Success Metrics

- **Key Performance Indicators:**
  - 100% of valid HL7 messages receive a valid HL7 ACK
  - 100% of requests are logged with `X-Message-Id` and status
  - <200ms median response time (excluding cold starts)
  - Chaos mode accurately simulates configured failure rates

- **User Engagement Metrics:**
  - Number of test runs using the mirror
  - Frequency of chaos mode usage

- **Business Metrics:**
  - Reduction in integration test cycle time
  - Fewer production incidents due to improved test coverage

---

## 8. Technical Considerations

- **Platform Requirements:**
  - Azure Functions (Consumption Plan)
  - .NET 10 Isolated Worker

- **Technology Preferences:**
  - .NET 10, C#
  - Application Insights for telemetry
  - IOptions<T> for config

- **Third-party Dependencies:**
  - None required for v1; may use HL7 parsing libraries if needed

- **Data Requirements:**
  - No persistent storage; all data is transient and logged

---

## 9. Timeline & Milestones

- **MVP Timeline:**
  - 2 weeks for initial implementation and deployment

- **Key Milestones:**
  - Week 1: Endpoint, ACK generation, logging
  - Week 2: Chaos mode, Application Insights, documentation

- **Dependencies:**
  - Azure subscription and resource group
  - Access to Application Insights
  - Function key management

---

## 10. Assumptions & Risks

- **Key Assumptions:**
  - All test clients can support Azure Function key authentication
  - HL7 v2 message format is sufficient for all test scenarios

- **Identified Risks:**
  - Misconfiguration of chaos mode could disrupt test runs
  - Cold start latency may affect load test accuracy
  - HL7 parsing errors if messages deviate from expected format

- **Open Questions:**
  - Should chaos mode support more granular error simulation (e.g., malformed ACKs)?
  - Is there a need for a UI/dashboard for real-time monitoring?
  - Will future versions require message persistence or replay?

---
