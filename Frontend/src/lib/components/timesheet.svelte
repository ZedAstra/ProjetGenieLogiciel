<script lang="ts">
	import type { HTMLBaseAttributes } from "svelte/elements";
    import { lightColors } from "$lib/colors";
    import * as Tooltip from "$lib/components/ui/tooltip/index.js";

    type Entry = {
        id: string;
        début: Date;
        fin: Date;
        nom: string,
        description: string,
        etat: "EnAttente" | "EnCours" | "Terminé" | "Annulé"
    }
    type Props = {
        currentMonth?: Date;
        entries?: Entry[];
    } & HTMLBaseAttributes;

    function getRandomColor() {
        return lightColors[Math.floor(Math.random() * lightColors.length)];
    }
    function getColor(task: Entry) {
        console.log(task.fin.getTime() * 1000, new Date().getTime() * 1000);
        if(task.etat == "EnAttente") return "#f7d794";
        else if(task.etat == "EnCours") return "#f3a683";
        else if(task.etat == "Terminé") return "#778beb";
        else if(task.etat == "Annulé") return "#f78fb3";

        else return "";
    }

    function getDaysInMonth(month: Date) {
        return new Date(month.getFullYear(), month.getMonth() + 1, 0).getDate();
    }
    
    let { 
        currentMonth = new Date, 
        entries = [] }: Props = $props();

    let daysInMonth = $derived(getDaysInMonth(currentMonth));
    function durée(entry: Entry) {
        return (entry.fin.getTime() - entry.début.getTime()) / (1000 * 60 * 60 * 24);
    }
    function isTodayDay(day: number) {
        return new Date().getDate() === day + 1;
    }
    function isToday(date: Date) {
        return new Date().getFullYear() == date.getFullYear() && new Date().getMonth() == date.getMonth() && new Date().getDate() == date.getDate();
    }
    
</script>

<div class="max-w-[calc(100%-255.2px)] overflow-scroll">
    <div class="grid grid-flow-col">
        {#each Array.from({ length: daysInMonth }) as _, i}
            {#if isTodayDay(i)}
            <div class="h-6 bg-blue-200 min-w-[2in] hover:cursor-pointer" style="width: 2in">
                <div class="text-xs p-0 m-0 align-middle">
                    {i + 1}
                </div>   
            </div>
            {:else}
            <div class="h-6 min-w-[2in]" style="width: 2in">
                <div class="text-xs p-0 m-0 align-middle">
                    {i + 1}
                </div>   
            </div>
            {/if}
        {/each}
    </div>

    {#each entries as entry}
        <div class="relative" style="height: 32px; width:{durée(entry)*2}in; left: {(entry.début.getDate() - 1) * 2}in; background-color: {getColor(entry)}">
            <Tooltip.Provider>
                <Tooltip.Root>
                    <Tooltip.Trigger class="absolute top-0 left-0 h-full w-full font-bold">
                        {entry.nom}
                    </Tooltip.Trigger>
                    <Tooltip.Content>
                        <p>{entry.description}</p>
                    </Tooltip.Content>
                </Tooltip.Root>
            </Tooltip.Provider>
            <!-- <button class="absolute top-0 left-0 h-full w-full">
                <div class="text-xs p-0 m-0 align-middle">
                    {entry.nom}
                </div>
            </button> -->
        </div>
    {/each}
</div>