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
Contrast
*/}}
{{- define "helpers.contrast.environment" -}}
{{- if .enableTracing }}
- name: CORECLR_ENABLE_PROFILING
  value: "1"
- name: CORECLR_PROFILER_PATH_64
  value: "/app/contrast/runtimes/linux-x64/native/ContrastProfiler.so"
- name: CORECLR_PROFILER
  value: "{8B2CE134-0948-48CA-A4B2-80DDAD9F5791}"
- name: CONTRAST__APPLICATION__NAME
  value: {{ .applicationName }}
- name: CONTRAST__SERVER__NAME
  value: {{ .server }}
- name: CONTRAST__SERVER__ENVIRONMENT
  value: "{{ .environment }}"
- name: CONTRAST__API__SERVICE_KEY
  value:  {{ .servicekey }}
- name: CONTRAST__API__API_KEY
  value: {{ .apikey }}
- name: CONTRAST__API__URL
  value: {{ .apiurl }}
- name: CONTRAST__API__USER_NAME
  value: {{ .username }}
- name: CONTRAST__AGENT__LOGGER__LEVEL
  value: "TRACE"
- name: CONTRAST_CORECLR_LOGS_DIRECTORY
  value: "/app/contrast/contrast_agent"
{{- end }}
{{- end -}}

{{/*
Service Ownership
*/}}
{{- define "helpers.serviceOwnership" -}}
tags.oakbrook.com/service_owner: {{ .Values.serviceOwner }}
{{- end -}}
