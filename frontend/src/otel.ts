import {
  CompositePropagator,
  W3CBaggagePropagator,
  W3CTraceContextPropagator,
} from "@opentelemetry/core";
import { WebTracerProvider } from "@opentelemetry/sdk-trace-web";
import { SimpleSpanProcessor } from "@opentelemetry/sdk-trace-base";
import { registerInstrumentations } from "@opentelemetry/instrumentation";
import { getWebAutoInstrumentations } from "@opentelemetry/auto-instrumentations-web";
import { resourceFromAttributes } from "@opentelemetry/resources";
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";

declare const __OTEL_EXPORTER_OTLP_ENDPOINT__: string;
declare const __OTEL_SERVICE_NAME__: string;

const FrontendTracer = async () => {
  // Browser telemetry disabled - OTLP relay getting 403 from Aspire dashboard
  // Server-side telemetry is still active and working
  console.log("Browser telemetry disabled");
  return;

  const { ZoneContextManager } = await import("@opentelemetry/context-zone");

  console.log(__OTEL_EXPORTER_OTLP_ENDPOINT__, __OTEL_SERVICE_NAME__);
  const provider = new WebTracerProvider({
    resource: resourceFromAttributes({
      [ATTR_SERVICE_NAME]: __OTEL_SERVICE_NAME__,
    }),
    spanProcessors: [
      new SimpleSpanProcessor(
        new OTLPTraceExporter({
          url: __OTEL_EXPORTER_OTLP_ENDPOINT__,
          headers: {},
        }),
      ),
    ],
  });

  const contextManager = new ZoneContextManager();

  provider.register({
    contextManager,
    propagator: new CompositePropagator({
      propagators: [
        new W3CBaggagePropagator(),
        new W3CTraceContextPropagator(),
      ],
    }),
  });

  registerInstrumentations({
    tracerProvider: provider,
    instrumentations: [
      getWebAutoInstrumentations({
        "@opentelemetry/instrumentation-fetch": {
          propagateTraceHeaderCorsUrls: /.*/,
          clearTimingResources: true,
          applyCustomAttributesOnSpan(span) {
            span.setAttribute("app.synthetic_request", "false");
          },
        },
      }),
    ],
  });
};

export default FrontendTracer;
