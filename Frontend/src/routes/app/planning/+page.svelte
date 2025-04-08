<script lang="ts">
    import { getLocalTimeZone, today } from "@internationalized/date";
	import { appFetch } from "$lib/app.svelte";
	import { Button } from "$lib/components/ui/button";
    import * as Card from "$lib/components/ui/card";
    import { Input } from "$lib/components/ui/input";
    import Timesheet from "$lib/components/timesheet.svelte";
    import { RangeCalendar } from "$lib/components/ui/range-calendar";
    import * as Dialog from "$lib/components/ui/dialog";

    import { getAppState } from "$lib/app.svelte";
	import { goto } from "$app/navigation";
	import { onMount } from "svelte";

    if(!getAppState().isLoggedIn)  goto("/");

    let tasks: {
        id: string,
        début: Date,
        fin: Date,
        nom: string,
        description: string,
        etat: "EnAttente" | "EnCours" | "Terminé" | "Annulé",
        assignés: any[]
    }[] = $state([]);

    onMount(init);

    async function init() {
        var response = await appFetch("planning/of", {
            method: "GET",
            headers: {
                "Time": (new Date).toISOString()
            }
        });
        if(response.ok) {
            tasks = await response.json();
            for(let task of tasks) {
                task.début = new Date(task.début);
                task.fin = new Date(task.fin);
            }
        }
    }

    function durée(entry: typeof tasks[0]) {
        return (entry.fin.getTime() - entry.début.getTime()) / (1000 * 60 * 60 * 24);
    }

    let newTaskOpen = $state(false);
    let newTaskData = $state({
        nom: "",
        description: "",
        début: "",
        fin: "",
        assignés: []
    });

    const start = today(getLocalTimeZone());
    const end = start.add({days: 7});

    let newTaskDateRange = $state({
        start,
        end
    });
    function newTask() {
        newTaskData = {
            nom: "",
            description: "",
            début: "",
            fin: "",
            assignés: []
        };
        newTaskOpen = true;
    }
    function createTask() {
        newTaskOpen = false;
        newTaskData.début = newTaskDateRange.start.toString();
        newTaskData.fin = newTaskDateRange.end.toString();
        appFetch("planning/create", {
            method: "POST",
            body: JSON.stringify(newTaskData),
            headers: {
                "Content-Type": "application/json"
            }
        }).then((response) => {
            if(response.ok) {
                init();
            }
        });
    }
</script>

<div class="flex flex-col overflow-x-hidden">
    <div class="prose">
        <h1>Planning</h1>
    </div>
    <Timesheet entries={tasks}></Timesheet>
    <div class="flex flex-col flex-wrap py-4 gap-4 overflow-x-hidden">
        <Dialog.Root bind:open={newTaskOpen}>
            <Dialog.Content class="w-[calc(100vw-32px)] min-w-[calc(100vw-32px)]  p-4">
                <Dialog.Header>
                    <Dialog.Title>Nouvelle tache</Dialog.Title>
                </Dialog.Header>
        
                <div class="flex flex-col gap-2">
                    <Input
                        type="text"
                        placeholder="Nom de la tâche"
                        bind:value={newTaskData.nom}
                        class="w-full"/>
                    <Input
                        type="text"
                        placeholder="Description de la tâche"
                        bind:value={newTaskData.description}
                        class="w-full"/>
                    <RangeCalendar
                        class="w-full"
                        bind:value={newTaskDateRange} />

                    <Button
                        variant="default"
                        class="w-full"
                        onclick={createTask}>
                        Créer</Button>
                </div>
            </Dialog.Content>
        </Dialog.Root>
        <Button variant="default" class=" w-64" onclick={newTask}>
            Nouvelle tâche +
        </Button>
        {#each tasks as task}
            <Card.Root class="max-w-96">
                <Card.Header>
                    <Card.Title>{task.nom}</Card.Title>
                    {task.description}
                </Card.Header>
                <Card.Content>
                    <div class="flex flex-row">
                        <div class="flex-1">
                            <div class="text-xs">Début: {task.début.toLocaleDateString()}</div>
                            <div class="text-xs">Fin: {task.fin.toLocaleDateString()}</div>
                        </div>
                        <div class="flex-1">
                            <div class="text-xs">Durée: {durée(task)} jours</div>
                            <div class="text-xs">Etat: {task.etat}</div>
                        </div>
                    </div>
                </Card.Content>
            </Card.Root>
        {/each}
    </div>
</div>