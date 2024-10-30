var connectionScore = new signalR.HubConnectionBuilder().withUrl("/hubs/scoreHub").build();

connectionScore.on("UpdateScore",
    (totalRuns, wickets, balls, run, batsmanName, batsmanImage, bowlerName, bowlerImage, teamA, teamB) => {
        var overs = Math.floor(balls / 6);
        var ballsInOver = balls % 6;
        document.getElementById("BatsmanImage").src = batsmanImage;
        document.getElementById("BatsmanName").innerText = batsmanName;
        document.getElementById("BowlerImage").src = bowlerImage;
        document.getElementById("BowlerName").innerText = bowlerName;
        document.getElementById("TotalRuns").innerText = totalRuns.toString();
        document.getElementById("TotalWickets").innerText = wickets.toString();
        document.getElementById("TotalOvers").innerText = `${overs}.${ballsInOver}`;
        document.getElementById("TotalTarget").innerText = target.toString();
        document.getElementById("RemainingScore").innerText = target - totalRuns;
        document.getElementById("RemainingBalls").innerText = 30 - balls;
        document.getElementById("target").value = totalRuns;

        var container = document.getElementById(`over${overs}`);
        if (ballsInOver == 0) {
            container = document.getElementById(`over${overs - 1}`);
        }

        if (container) {

            var bg = "";

            if (run == -1) {
                bg = "bg-danger";
            } else if (run == 4 || run == 6) {
                bg = "bg-success";
            }

            const ballContainer = document.createElement("div");
            ballContainer.className = `ball-container ${bg} p-2 m-1 border rounded align-items-center justify-content-center`;
            container.appendChild(ballContainer);

            const ballP = document.createElement("p");
            if (run == 4 || run == 6 || run == -1) {
                ballP.className = "text-light";
            }
            if (run == -1) {
                ballP.innerHTML = "W";
            } else {
                ballP.innerHTML = run;
            }
            ballContainer.appendChild(ballP);

            if (run != -1) {
                const ballLabel = document.createElement("p");
                if (run == 4 || run == 6 || run == -1) {
                    ballLabel.className = "text-light";
                }
                ballLabel.innerHTML = "Runs";
                ballContainer.appendChild(ballLabel);
            }
        }

        if (totalRuns >= target) {
            document.getElementById("WinningTeam").innerText = teamA;
            document.getElementById("WinningCause").innerText = (5 - wickets).toString() + " Wicket(s)";
        } else if (totalRuns == target - 1) {
            document.getElementById("WinningTeam").innerText = teamA + " & " + teamB;
            document.getElementById("WinningCause").innerText = " Draw";
        } else {
            document.getElementById("WinningTeam").innerText = teamB;
            document.getElementById("WinningCause").innerText = (target - totalRuns - 1).toString() + " Run(s)";
        }

    });

async function startInnings() {
    const promises = [];
    for (i = 0; i < 5; i++) {
        promises.push(connectionScore.invoke("StartInnings", teamId, opponenetTeamId, target)
            .catch(err => console.error(err.toString())));
    }
    await Promise.all(promises);
}

connectionScore.start().then(async () => {
    await connectionScore.send("ResetScore").catch(err => console.error(err.toString()));
    await startInnings();
    if (target == 999) {
        document.getElementById("start-second-innings").removeAttribute("hidden");
    } else {
        document.getElementById("final-result").removeAttribute("hidden");
        document.getElementById("go-back").removeAttribute("hidden");
    }
    connectionScore.send("ResetScore").catch(err => console.error(err.toString()));
}).catch(err => console.error(err.toString()));
