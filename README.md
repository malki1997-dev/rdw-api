# API de fiabilité des véhicules (RDW)

[![CI](https://github.com/malki1997-dev/rdw-api/actions/workflows/ci.yml/badge.svg)](https://github.com/malki1997-dev/rdw-api/actions/workflows/ci.yml)

API REST ASP.NET Core qui expose les taux de défaut au contrôle technique (APK) des voitures particulières néerlandaises, par marque, modèle, carburant et âge. Elle lit des tables résumées produites par le [pipeline de données](https://github.com/malki1997-dev/rdw-car-reliability-pipeline) (83,5 millions de lignes du RDW traitées avec Spark, dbt et Airflow).

**Définition :** le taux de défaut (`defectRatePct`) est la part des contrôles APK qui ont au moins un défaut.

## Endpoints

Toutes les routes sont sous `/api/v1`. Sauf `/health`, elles exigent l'en-tête `X-Api-Key`.

| Méthode | Route | Description |
| --- | --- | --- |
| GET | `/health` | État de l'API et de la base (public) |
| GET | `/models` | Liste paginée des modèles, avec filtres |
| GET | `/brands` | Classement par marque, avec l'âge moyen |
| GET | `/models/{brand}/{model}/age-curve` | Taux de défaut par âge, une courbe par carburant |

**Paramètres de `/models`**

| Paramètre | Défaut | Rôle |
| --- | --- | --- |
| `brand` | aucun | Filtre exact sur la marque (insensible à la casse) |
| `model` | aucun | Filtre « contient » sur le modèle |
| `fuelType` | aucun | `Petrol`, `Diesel`, `Hybrid`, `Electric`, `LPG`, `Hydrogen`, `Other` |
| `minInspections` | 100 | Nombre minimal de contrôles pour apparaître |
| `sort` | `defect_rate` | `defect_rate` (taux croissant) ou `inspections` (volume décroissant) |
| `page`, `pageSize` | 1, 20 | Pagination ; `pageSize` est limité à 100 |

`/brands` accepte `minInspections` (10 000 par défaut). `/age-curve` accepte `fuelType`.

**Codes d'erreur :** `400` paramètres invalides (page, pageSize ou sort), `401` clé API absente ou invalide, `404` modèle inconnu pour `/age-curve`.

## Exemple

```bash
curl -H "X-Api-Key: VOTRE_CLE" "http://localhost:5294/api/v1/models?brand=toyota&pageSize=1"
```

```json
{
  "page": 1,
  "pageSize": 1,
  "total": 100,
  "items": [
    {
      "brand": "TOYOTA",
      "model": "YARIS CROSS",
      "fuelType": "Hybrid",
      "nbInspections": 10491,
      "defectRatePct": 15.1,
      "avgAgeYears": 3.8,
      "avgDefects": 0.34
    }
  ]
}
```

## Lancer en local

Prérequis : SDK .NET 10 et la base PostgreSQL du pipeline (tables `marts.agg_reliability_by_model` et `marts.agg_reliability_by_age`), avec un compte en lecture seule.

```bash
cd RdwApi
dotnet user-secrets set "ConnectionStrings:Rdw" "Host=localhost;Port=5433;Database=rdw;Username=rdw_api;Password=VOTRE_MOT_DE_PASSE"
dotnet user-secrets set "ApiKey" "une-cle-longue-et-aleatoire"
dotnet run --launch-profile http
```

En développement, une documentation interactive (Scalar) est disponible sur `/scalar`.

## Sécurité

- **Clé API** dans l'en-tête `X-Api-Key`, comparée à temps constant ; l'API refuse de démarrer sans clé.
- **Moindre privilège :** compte PostgreSQL en lecture seule sur deux tables seulement.
- **Secrets** hors du dépôt : User Secrets en développement, variables d'environnement en production.
- **Pagination plafonnée** à 100 résultats par requête.

## Lire les résultats avec prudence

- **Taux de défaut n'est pas fiabilité.** Les défauts les plus fréquents sont des pneus et des éclairages, donc de l'entretien. Le kilométrage n'est pas disponible.
- **Biais d'âge.** Comparer deux modèles sans tenir compte de l'âge fausse la conclusion : utiliser `/age-curve`, et lire `avgAgeYears` dans les autres réponses.
- **Petits échantillons.** `nbInspections` est fourni pour juger la fiabilité du taux : avec 162 contrôles, la marge d'erreur est d'environ 5,6 points ; avec plus de 10 000, elle est inférieure à 1 point.
- **Données non temps réel.** Elles sont recalculées à chaque exécution mensuelle du pipeline.
- Un même nom de modèle peut regrouper plusieurs générations.

## Technique

.NET 10, ASP.NET Core, EF Core avec Npgsql (entités sans clé, lecture seule), OpenAPI et Scalar. Intégration continue : build avec GitHub Actions.

## État

L'API n'est pas encore déployée en ligne.
