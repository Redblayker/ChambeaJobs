# Propuestas de mejoras y nuevos módulos — ChambeaJobs

**Enfoque:** Candidato, Empresa y Monetización

Autor: Equipo ChambeaJobs
Fecha: 2026-08-18

---

Índice
1. Introducción
2. Resumen técnico y hallazgos
3. Propuestas centradas en Candidato
4. Propuestas centradas en Empresa
5. Propuestas de Monetización y Paquetes
6. Priorización y roadmap
7. Checklist técnico y mapeo al código existente
8. Estimaciones y recursos
9. Riesgos y mitigaciones
10. Anexos (archivos y referencias específicas del repo)

1. Introducción
Objetivo: presentar mejoras y nuevos módulos priorizados para aumentar captación y retención de candidatos, mejorar el valor propuesto a empresas y optimizar fuentes de ingreso (monetización), partiendo del código y arquitectura actual de ChambeaJobs (evidencias: Program.cs, controllers, capas Application/Infrastructure).

2. Resumen técnico y hallazgos
- Arquitectura actual: ASP.NET Core MVC + EF Core + Identity; SignalR para tiempo real; patrón Repository/UnitOfWork; hosted services para jobs; almacenamiento local para archivos (IFileStorageService implementado como LocalFileStorageService).
- Módulos observados: Candidato, Empresa, Vacantes, Postulaciones, Favoritos, Admin (auditoría, reportes), Finanzas, CRM, Chat/Soporte/Chatbot, Evaluaciones; background jobs para recordatorios, renovaciones y automatizaciones; seguridad robusta (DataProtection, CSP).
- Oportunidades: mejorar experiencias del candidato, ofrecer servicios atractivos a empresas, y crear flujos de monetización escalables.

3. Propuestas centradas en Candidato (UX, productividad y valor percibido)

3.1 Perfil candidato enriquecido (perfil "one-page" con skills, video-CV, portfolio)
- Descripción: permitir que el candidato construya un perfil público completo con extracción automática de skills desde CV y opción de subir video-CV.
- Valor: reduce fricción para postular y mejora matching; aumenta visibilidad del candidato.
- Implementación: nuevo DTO/Entity en Domain (PerfilCandidato), UI de edición en Views/Candidato, servicios en Application (ICvGeneradorService ya existe) y almacenamiento en IFileStorageService.

3.2 Video-CV + playback optimizado
- Descripción: permitir grabar y subir video-CV, convertir/transcodificar y generar thumbnails; reproducir desde URLs seguras.
- Valor: diferenciador, mejor evaluación previa.
- Implementación técnica: añadir provider cloud storage (Azure Blob / AWS S3), servicio de transcodificación (job off-line o servicio externo), cambiar LocalFileStorageService a implementación configurable.

3.3 Parsing automático de CV y extracción de skills (NLP)
- Descripción: extraer educación, experiencia, skills y roles desde documentos (PDF/DOCX) o texto.
- Valor: mejora precisión del matching, filtros automáticos para empleadores.
- Implementación: servicio IParserCvService; pipeline off-line que guarda entidades en DB y alimenta motor de recomendaciones.

3.4 Matching inteligente y recomendaciones personalizadas
- Fase 1 (rápida): heurística (score por skills, ubicación, experiencia, recency).
- Fase 2: modelo ML (colaborativo o embeddings + vector DB) para recomendaciones.
- Integración: nuevo IRecomendacionService; jobs periódicos para reentrenar modelo; usar eventos (VacantePublicadaEvento, PerfilActualizadoEvento) para señales.

3.5 Experiencia móvil / PWA y notificaciones push
- Descripción: convertir partes críticas en PWA (aplicación instalable) y añadir web push (VAPID).
- Valor: aumenta re-engagement, alertas de nuevas vacantes o respuestas.
- Implementación: service worker, endpoints para registrar suscripciones y enviar notificaciones push desde backend; SignalR sigue para realtime in-app.

3.6 Evaluaciones y micro-certificaciones
- Descripción: pruebas cortas (quiz / coding snippets) con badges en perfil.
- Valor: candidatos demuestran skills -> empresas filtran mejor.
- Implementación: módulo EvaluacionService y UI; resultados guardados y visibles en perfil.

4. Propuestas centradas en Empresa (valor, eficiencia y analítica)
4.1 Panel de empresa con métricas y pipeline de candidatos
- Descripción: dashboard con KPIs: vistas de vacantes, tasa de postulación, tiempo promedio hasta contratación, conversiones por paquete.
- Valor: retiene clientes y justifica gasto en paquetes.
- Implementación: ampliar IFinanzasDashboardService e IReporteService; nuevas vistas en AdminEmpresaService.

4.2 Herramientas de promoción de vacantes (destacar, paquetes y boosts)
- Descripción: permitir destacar vacantes, promoción en newsletter, o promoción por geografía/skill-targeting.
- Valor: ingresos incrementales, mayor satisfacción de empresas.
- Implementación: extender PaqueteEmpresaService y tablas de promoción; integraciones con pagos.

4.3 Integración ATS / APIs y Webhooks
- Descripción: exponer API REST (versionada) y webhooks para eventos (nueva postulación, pago aprobado) para integrar ATS/ERP/CRMs.
- Valor: permite integración con flujos de RRHH corporativos; argumento para planes empresariales.
- Implementación: crear proyecto API (o versionar controladores actuales con [ApiController]); seguridad con OAuth2 / API keys; documentar con Swagger.

4.4 Candidate Screening y entrevistas en la plataforma
- Descripción: herramientas para screening (preguntas, pruebas técnicas) y agenda de entrevistas (con Jitsi para videollamadas).
- Valor: reduce tiempo de contratación y aumenta uso de la plataforma durante el proceso.
- Implementación: integrar scheduling -> generar enlaces Jitsi (ya en CSP); guardar registros en Postulacion entity.

4.5 Employer CRM mejorado
- Descripción: sistema de etiquetas, notas internas por candidato, pipeline de contratación y automatización de mensajes.
- Valor: empresas gestionan candidatos sin salir de la plataforma.
- Implementación: ampliar módulo CrmService y modelos para notas/labels; usar bus de eventos para automations.

5. Propuestas de Monetización y Paquetes (modelos y flujos)
5.1 Modelo freemium + paquetes por empresa
- Ofrecer:
  - Gratis: 1 vacante activa, búsquedas limitadas.
  - Básico: N vacantes, filtros avanzados.
  - Premium: vacantes destacadas, acceso a CV completos, analytics, integraciones API.
- Implementación: reglas en PaqueteEmpresaService, UI para compra en empresa, manejo de expiración y renovación (hosted services ya en Program.cs).

5.2 Micropagos por promoción de vacantes y boosts
- Pago por destacar vacante por X días o por campañas geotarget.
- Integración de pago: Stripe recomendado por experiencia global; usar webhooks idempotentes para confirmaciones.

5.3 Suscripción para acceso a base de candidatos / créditos
- Sistema de créditos para descargar datos de CVs (o contactar candidatos directos).
- Control de consumo: contabilizar mediante PostulacionService/CompatibilidadService.

5.4 Servicios profesionales y marketplace
- Servicios: revisión de CV, entrevistas simuladas, tests premium.
- Marketplace: permitir proveedores (evaluadores, headhunters) que ofrezcan servicios cobrables dentro de la plataforma (modelo revenue-share).

5.5 Programas de referidos y descuentos por volumen
- Incentivar referidos de empresas y candidatos con créditos o descuentos.

6. Priorización y roadmap (foco en impacto para candidatos y empresas)

Sprint 0 — Infra y seguridad (1-2 semanas)
- Añadir Cloud Storage provider (Azure/AWS) + refactor IFileStorageService.
- Observabilidad: integrar Sentry y métricas (App Insights).
- Dockerfile & pipeline CI básico.

Sprint 1 — Experiencia candidato esencial (2-3 semanas)
- Perfil enriquecido (skills + videoCV upload).
- Parsing inicial de CV (fase heurística).
- PWA básico + notificaciones.

Sprint 2 — Panel empresa y monetización básica (2-3 semanas)
- Dashboard empresa (KPIs básicos).
- Modelos de paquetes (Implementar pago con Stripe en sandbox).
- Webhooks y API básica.

Sprint 3 — Matching + evaluación (3-4 semanas)
- Motor de recomendaciones heurístico.
- Banco de pruebas y micro-certificaciones.
- Integración de entrevistas (Jitsi scheduling).

Sprint 4 — Marketplace y escalado (4-6 semanas)
- Marketplace de servicios.
- Optimización de video (transcodificación y CDN).
- Vector DB / embeddings (si se adopta RAG o búsqueda semántica).

7. Checklist técnico y mapeo al código existente (qué modificar y dónde)
- Program.cs
  - Reemplazar registration de IFileStorageService por fábrica/configurable para Local/Azure/AWS.
  - Registrar HttpClient para servicios externos (Stripe, AI/NLP).
  - Mantener y ampliar bus de eventos (Application.Eventos) para publicar webhooks y alimentar jobs de ML.
- ChambeaJobs.Infrastructure
  - Añadir Storage/AzureBlobStorageService y/o S3StorageService.
  - Añadir servicios de procesamiento (TranscoderService, ParserCvService).
  - Integrar Sentry/ErrorReportingService.
- ChambeaJobs.Application
  - Nuevos interfaces: IRecomendacionService, IParserCvService, IEmployerDashboardService, IPaymentService.
  - DTOs y events: VacantePromocionadaEvento, CandidateViewedEvento, PagoCompletadoEvento.
- ChambeaJobs.Web
  - Vistas y controllers: extender CandidatoController, EmpresaController y crear API controllers (api/empresa, api/candidato).
  - Añadir endpoints para web push, service worker y API de subida chunked si se exige gran tamaño.

8. Estimaciones y recursos (muy aproximadas)
- Refactor IFileStorageService + migración a Blob: 3–7 días (1 dev)
- PWA + web push: 5–10 días (1 dev)
- Perfil enriquecido + parsing CV básico: 7–14 días (1–2 dev)
- Dashboard empresa + Stripe integration: 7–12 días (1–2 dev)
- Motor de recomendaciones heurístico: 7–14 días (1 dev)
- Implementación marketplace (fase 1): 10–20 días (2 dev)

9. Riesgos y mitigaciones
- Costo de almacenamiento y transcodificación: mitigar con lifecycle policies en Blob/S3 y usar CDN.
- Privacidad y cumplimiento: solicitar consentimiento para procesamiento de CV; añadir auditoría y eliminación de datos.
- Fraude en pagos: usar Stripe Radar y validación en webhooks.
- Escalabilidad de realtime (SignalR): usar backplane (Redis) si se escala a múltiples instancias.

10. Anexos y referencias al código
- Archivos inspeccionados:
  - ChambeaJobs.Web/Program.cs — registro de servicios, hosted services, CSP y DataProtection.
  - Controllers: AccountController.cs, CandidatoController.cs, EmpresaController.cs, VacanteController.cs, NotificacionesController.cs, ChatbotController.cs, SoporteController.cs, Admin*.
  - Estructura de proyectos: ChambeaJobs.Application, ChambeaJobs.Infrastructure, ChambeaJobs.Domain, ChambeaJobs.Web.
- Recomendaciones de primer commit (ejemplos de cambios a realizar):
  - Añadir appsetting "Storage:Provider": "Local|Azure|S3" y configuración de credenciales en secrets.
  - Implementar AzureBlobStorageService en Infrastructure/Storage y cambiar Program.cs para DI condicional.
  - Agregar proyecto ChambeaJobs.API (opcional) para exponer contratos REST.

Resumen ejecutivo (1 página)
- Prioridad inmediata: mover archivos a storage cloud, integrar pagos (Stripe), mejorar perfil candidato (videoCV + parsing) y crear dashboard empresa con métricas.
- Beneficio esperado: incremento en retención de candidatos, mayor satisfacción empresarial y nuevas fuentes de ingresos recurrentes.

---

Nota: He subido este documento en formato Markdown para preservación y fácil edición. Si deseas el archivo PDF directamente en el repositorio, puedo generar el PDF a partir de este Markdown y subirlo como /docs/ChambeaJobs_Propuestas_CandidatoEmpresaMonetizacion.pdf en un commit siguiente. ¿Deseas que proceda a generar y subir el PDF ahora?