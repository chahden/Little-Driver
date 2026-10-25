# Pho Dining Set - free sampler

9 kits, 42 meshes, 18,010 triangles, 42 prefabs. Metres, Z-up authored, pivots at the foot of each prop. One material, one draw call each.

A slice of the full pack "Asian Street Food Carts & Stalls" (66 kits, 337 meshes), cut from the same build and held to the same bar - nothing here is a lower-quality teaser version.

## Requirements

- Unity 6000.5 or newer. Built and verified on 6000.5.9f1.
- URP or Built-in. This project ships with URP as its render pipeline; the shader graph also targets Built-in, so importing the .unitypackage into a Built-in project works too. HDRP is not supported in v1.
- `com.unity.shadergraph`: URP already depends on it. On a Built-in project, install it from Package Manager BEFORE importing, otherwise `SH_Pack01_Lit` cannot compile and every material falls back to pink.

## What is inside

- Clutter: 7
- Cook: 28
- Furniture: 7

Scenes:

- `Overview`: every prefab in a grid, 42 instance
- `PhoDiningSet`: 46 instance

## Materials

- `M_Pack01_Atlas`: every mesh in this sampler. Colour atlas plus a metallic-roughness atlas, both 1024.

Atlas textures use Point filtering on purpose: the palette is flat swatches, and bilinear bleeds neighbouring colours across swatch edges.

## Shader properties

`SH_Pack01_Lit` is one Shader Graph targeting Built-in and URP; every property below shows up in the material Inspector.

| Property | Reference | Type | Default | What it does |
|---|---|---|---|---|
| Base Texture | `_BaseTexture` | Texture2D, white | palette atlas | Sampled on UV0, multiplied by Base Color, drives Base Color. Flagged as the main texture. |
| Base Color | `_BaseColor` | Color | 1, 1, 1, 1 | Tint over the base texture. Its alpha drives Alpha, so a material on this graph can be made transparent. |
| Use MR Texture | `_UseMRTexture` | Boolean | on | On, metallic and smoothness come from MR Texture; off, from the two sliders below. |
| MR Texture | `_MRTexture` | Texture2D, white | MR atlas | glTF channel layout: `g` is roughness, `b` is metallic, `r` unused. Import it as Linear, not sRGB. |
| Roughness | `_Roughness` | Slider 0-1 | 0.65 | Read only when Use MR Texture is off. Reaches Smoothness as `1 - roughness`. |
| Metallic | `_Metallic` | Slider 0-1 | 0.0 | Read only when Use MR Texture is off. |

Surface Type is exposed through Allow Material Override on both targets, so a material on this graph switches to Transparent without a second shader.

## Holes are real geometry

No alpha cutout anywhere. Openings in shutters, grilles and breeze blocks are modelled, so they hold up at any camera angle and cost nothing in overdraw.

## Licence

Use in personal and commercial projects, no royalty and no attribution required. Redistributing the asset files themselves is not allowed. Full terms in `LICENSE.txt`.

## The full pack

Asian Street Food Carts & Stalls - 66 kits, 337 meshes: four modular vendor carts,
the food and cookware that dress them, furniture, signage, and snap-together street
modules. Same build, same atlas, same budgets. From the same publisher.
