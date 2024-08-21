# Horizon
[![Continous Integration](https://github.com/tabman83/horizon/actions/workflows/dotnet.yml/badge.svg)](https://github.com/tabman83/horizon/actions/workflows/dotnet.yml)

A .NET Kubernetes Operator that securely synchronizes secrets from Azure Key Vault to Kubernetes in real-time using Event Grid webhooks

## Description

The Horizon Kubernetes Operator has been developed in C# for .NET 8.0 to make Azure Key Vault objects available to Kubernetes.
The features I was looking for were:
- To avoid a direct program dependency on Azure Key Vault for getting secrets, and adhere to the 12 Factor App principle for configuration (https://12factor.net/config)
- To make it simple, secure and low risk to transfer Azure Key Vault secrets into Kubernetes as native Kubernetes secrets
- To enable workloads to get newly added secrests in real-time
- To avoid using polling against Azure Key Vaults to ensure Kubernetes secrets are up to date

Because none of the solutions I found on the internet did fit my bill, I decided to develop my own one.

## Installation

Before running the operator you need to deploy the CRD to your cluster:
```bash
kubectl apply -f crd.yaml
```

This will enable the cluster to accept instances of `AzureKeyVaultSubscription` resources. Below is an example:
```yaml
apiVersion: horizon.ninoparisi.io/v1
kind: AzureKeyVaultSubscription
metadata:
  name: ci-tooling
  namespace: ci-tooling
spec:
- azureKeyVaultName: tooling-ci-we
  k8sSecretObjectName: ci-tooling
```

The resource is namespaced and this implies the target kubernetes secret will be created on the same namespace.
The `spec` section defines the behavior: secrets are read from the `tooling-ci-we` Key Vault on Azure and stored to a 
Kubernetes secret object named `ci-tooling`. This section is an array therefore you can define multiple Azure Key Vaults
to end up in the same Kubernetes secret or different ones.

An authentication section will be added later to define what kind of authentication to use against the Azure Key Vault. Currently
the default is Managed Identity.

## Architecture

The controller is responsible for reconciling the cluster wether 2 changes happen:
- an `AzureKeyVaultSubscription` resource is added/modified/removed
- a webhook notification is received telling a new Key Vault secret version has been added

These flows are detailed below:

![image](https://www.plantuml.com/plantuml/png/VP4nRy8m48Lt_ufJkhG3i5OC5IrB9JfKGkhKnSG7M1XVP9zfuTVt9R6WKgKkYUFttRjtbqLMcxGSWr6lWQbvlfJ4Apv_s19qNJQvJRvJBv6iS-ncHt5-wt5m75ZPDSPPjHkRcGudihaw42ney6ZCHhwfMJrcMeQIbLD3Tsz-jzUNKDWGSKJhCzd3AQF-dmGDu5QY9WaatS2-Il8NYPznETu7JZrrZPIvJQm3kXwElpqwSOFoBfY2eqDEOuB0UYk9Jvp0zeqcawSntPo-hBRxNgPsR-EESuzCfrEyHWGeAkPLfdlhtAnqoCDrOGytKomCP6BhQiuX6KS50ktgzLUK3cAz1p0sgD-zNXKmcF46m67hT_sePe47_leF)

![image](https://www.plantuml.com/plantuml/png/NL4zJyCm4DtlLvpCt1rrg0eL0gaCI4Ymi3Z9DRLYV95z3j1VppcHISLat_kuzsJlazWe-TE3EF64vfsQC_E0WSMTN6l5SJ3GMR6DJOJ3X3QXkRlaI7Ya7topsOk1beD4zaWJ1UcZwsRPGvdmKKS33N-ZETucFFYSXAB1csVNd-NUP_gpypZxdZYw2uUFS5XmJ_6gGw8saip2r_cwne-y1B-m9bBeD1H0EsyskwjgsxeYZxgKEbYf8jGIM_nQtW5qADWmAHR9TjLQ4jK4IbJBFwOuTnNSBbcrF2n74MZbbLNloVMmr-hw9xy0)

## References
- [Azure Key Vault to Kubernetes](https://github.com/SparebankenVest/azure-key-vault-to-kubernetes)
- [Azure Key Vault throttling guidance](https://learn.microsoft.com/en-us/azure/key-vault/general/overview-throttling)
