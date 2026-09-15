{{/* vim: set filetype=mustache: */}}

{{- define "helpers.name" -}}
{{- lower .Values.name | trunc 63 | trimSuffix "-" | trimSuffix "." -}}
{{- end -}}

{{- define "helpers.fullname" -}}
{{- printf "%s-%s" .Values.name .Values.image.tag | lower | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "helpers.namespace" -}}
{{- default "default" .Values.namespace | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{- define "helpers.image" -}}
{{ printf "%s/%s:%s" .Values.image.repository .Values.image.imageName .Values.image.tag}}
{{- end -}}

{{- define "helpers.gsa" -}}
{{ printf "svc-%s-%s@%s%s.iam.gserviceaccount.com" (.Values.google.environment | trunc 1) (trimSuffix "service" (lower .Values.name)) .Values.google.projectPrefix .Values.google.environment }}
{{- end -}}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "helpers.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" | trimSuffix "." -}}
{{- end -}}

{{/*
Common labels
*/}}
{{- define "helpers.labels" -}}
app.kubernetes.io/name: {{ include "helpers.name" . }}
helm.sh/chart: {{ include "helpers.chart" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end -}}

{{/*
Datadog labels
*/}}
{{- define "helpers.datadog.labels" -}}
tags.datadoghq.com/env: {{ .Values.google.environment }}
tags.datadoghq.com/service: {{ include "helpers.name" . | quote }}
tags.datadoghq.com/version: {{ .Values.image.tag | quote }}
{{- end -}}

{{/*
Service Ownership
*/}}
{{- define "helpers.serviceOwnership" -}}
tags.oakbrook.com/service_owner: {{ .Values.serviceOwner }}
{{- end -}}
