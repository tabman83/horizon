# Horizon

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

This will enable the cluster to accept instances of `AzureKeyVaultSubscription`s. Below is an example:
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

![image](https://www.plantuml.com/plantuml/png/NL71JiCm3BtxAwnmxXtrC0qG0cb8890uSKbNqqNBwifn1kphSRgMJboYFB_t_5wKTL8lcWyHD6WaTA_aOwNXG9Y7rYiviDCtFX7kZ-WJkfjJ5i8D_U2xpr4TRBbNuOXqf5ux2Uqekoy87mmwQFWRtUOwuLtCIKaSNZs5RgUtCTEVdOVHzyWIJjn_zCy3YLUMYmQ9ja8wojx6wn8-y3dsXIsF-jOY0QnnLQqkYjNpYlFZMbYe8kJ155_Kx9ZtOA3CR4UDGhwThINHJ634RFd75ETg7jPJreHX6fl0AY-iUJz31skhRh_n2m00)

## References
- [Azure Key Vault to Kubernetes](https://github.com/SparebankenVest/azure-key-vault-to-kubernetes)
- [Azure Key Vault throttling guidance](https://learn.microsoft.com/en-us/azure/key-vault/general/overview-throttling)
