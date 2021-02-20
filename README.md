
Kubernetes webhook written in C#
=========

We can implement Kubernetes webhooks in any programming language when it provides an HTTPS powered web server. This repo shows how to implement one using C#.


To deploy, follow these steps:

1. Generate a server certificate using utilities like OpenSSL
1. Create a container image and push it to a repository 
1. Create a deployment and a service resource to host the webhook within the Kubernetes cluster 
1. Create a ValidateWebhookConfiguration resource in which you place the public key as CaBundle
1. Create a MutateWebhookConfiguration resource in which you place the public key as CaBundle
1. Test and try the webhook by creating a Pod resource. 
   1. Verify that there is an annotation automatically added by the webhook as implemented in the controllers
   1. Verify that you can not create any pod that uses images from repositories whose URLs start with `gcr.io`


