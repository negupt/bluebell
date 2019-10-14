#!/bin/sh
sed -i s#__loginserver__#cicd12.azurecr.io#g acr_sample.json
sed -i s#__scope__#/subscriptions/698e7133-d0be-44f4-bc25-76e2296b0fb0/resourceGroups/cicd12-rg-acr/providers/Microsoft.ContainerRegistry/registries/cicd12#g acr_sample.json
sed -i s#__tenantid__#72f988bf-86f1-41af-91ab-2d7cd011db47#g acr_sample.json
sed -i s#__regid__#/subscriptions/698e7133-d0be-44f4-bc25-76e2296b0fb0/resourceGroups/cicd12-rg-acr/providers/Microsoft.ContainerRegistry/registries/cicd12#g acr_sample.json
sed -i s#__regtype__#ACR#g acr_sample.json
sed -i s#__subid__#698e7133-d0be-44f4-bc25-76e2296b0fb0#g acr_sample.json
sed -i s#__subname__#My" "Microsoft" "Azure" "Internal" "Consumption#g acr_sample.json
sed -i s#__service_conn_name__#helium#g acr_sample.json
sed -i s#__type__#dockerregistry#g acr_sample.json
sed -i s#__url__#https://cicd12.azurecr.io#g acr_sample.json
sed -i s#__isShared__#false#g acr_sample.json