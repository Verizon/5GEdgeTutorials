variable "dd_api" {
  description = "This is the API Key for your Datadog account"
  sensitive = true
}

variable "dd_app" {
  description = "This is your Datadog application key"
  sensitive = true
}

variable "accountId" {
  description = "This is the account ID for your AWS account"
  default = <your-account-id>
}

variable "datadog_api_url" {
  default = "https://api.datadoghq.com/"
}


