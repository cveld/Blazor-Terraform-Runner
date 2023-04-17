# Blazor-Terraform-Runner
Run Terraform workflows from the browser.

The ultimate goal is to combine some features of Terraform Cloud and Azure Pipelines:

The solution should provide the user a dashboard where all terraform scopes are listed with the most recent commit that has been applied.

In its current state it has some half-baked features:
* ctrl-s support. It detects changes on the local disk (oops not yet cloud ready) and it starts a `terraform plan` operation.
* The progress is streamed into the browser by monitoring the log files progress on the local disk (oops again not yet cloud ready)
* The `terraform plan` operation is kicked off in a kubernetes cluster targeted by kubectl's current context. Local folders on disk are mounted as volumes for terraform configuration input, log output, terraform data and plan output.

Plans:
* Incorporate pretty plan: https://cloudandthings.github.io/terraform-pretty-plan/
* Move local disk interactivity to csi based volume interactivity
* Authentication/Authorization
* Git as the source for terraform source code
