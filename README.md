# Horizon

A .NET Kubernetes Operator that securely synchronizes secrets from Azure Key Vault to Kubernetes in real-time using Event Grid webhooks

## Create the CRD (only the very first time)

```bash
kubectl apply -f crd.yaml
```