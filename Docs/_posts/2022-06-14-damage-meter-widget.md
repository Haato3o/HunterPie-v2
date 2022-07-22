---
layout: post
title: "Damage Meter Widget"
date: 2022-06-14 23:21 -0300
badge: docs
categories: [Documentation, Overlay, Widgets]
---
The Damage Meter Widget is responsible for tracking and displaying yours and your party members damage and damage per second, as well as plotting everything in a graph for better understanding of your hunts.

## Widget Structure

![damage-meter-widget](https://media.discordapp.net/attachments/456629861637816340/986457318944276500/unknown.png) *Damage Meter widget demonstration*

## Damage Accuracy

### Monster Hunter Rise

To calculate the damage for Monster Hunter Rise, HunterPie will sum every hit damage and assign it to whoever did the damage. This does not consider:

- Environmental damage
- Damage done by palamutes/palicos
- Damage done by companions
- Damage done by ailments (e.g: poison, blast)
- Damage done by other monsters

## Settings

### Player colors

You can change yours and your party members color by going to the settings tab and clicking on the color configuration.

![color-settings](https://media.discordapp.net/attachments/456629861637816340/986458441780436992/unknown.png)

### Damage plot

The damage meter widget has a built-in plot graph, by default it shows your damage per second over time, however, you can also change it to display total damage instead.

> **Note:** Changing the plot mode while in a hunt will not update the previous points that were already plotted in the graph.
{: .prompt-warning }

### DPS Calculation

By default, HunterPie will calculate the damage per second based on the quest timer, however, this can be inaccurate especially when joining in-progress quests (such as SOSes), you can change the DPS calculation strategy in the Damage Meter settings.

Strategy | Description
---------|--------------------------
Relative to quest | The quest timer will be used to calculate players DPS
Relative to join | The time when each player joined the quest will be subtracted <br> from the quest timer when calculating players DPS
Relative to first hit | The time when each player hit a monster for the first time <br> will be subtracted from the quest timer when calculating players DPS

