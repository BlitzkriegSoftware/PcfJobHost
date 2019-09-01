# PcfJobHost #

The scheduled task host allows teams to dynamically load jobs by dropping them into the `Jobs` folder, and restarting the job engine.

## PCF Notes ##

This was made as an alternative to the PCF Scheduled tasks engine (which is good), but does not deal well with very long running jobs (> 10 minutes).

Instead, we use the great `Quartz` scheduling engine from **Marko Lahma** See: https://www.quartz-scheduler.net/

There is a sample `Manifest.yml` file, that you will want to "tune" for your own purposes. Depending on the number of jobs and their needs typically RAM needs to be increased. Likewise you may want to add your own Services, Environment Variables, etc.

The simple `bash` script `pcfit.sh` will build and deploy the job engine to the current API, ORG, and SPACE.

## Sample Jobs ##

See the example in `ExampleJob` project.

Notes:

> You can only throw exceptions of type `JobExecutionException` so wrap the entire `Execute()` code in a try catch and re-throw all exceptions wrapped in this exception.

> Data (typically configuration) is passed to jobs via the `JobDataMap`

## Job Engine ##

# Jobs sub-folder #

This is where jobs go. Don't delete placeholder text file, as it will force the `Jobs` folder to exist. Notice that is is set to `Content` and `Copy Always`

## C# Jobs Structure ##

See the `ExampleJob.cs` file.

Notice they inherit from Quartz's `IJob` interface.

## Building your jobs ##

When your Job Assembly builds, copy output to the .\Jobs sub-folder

for example, you could implement a post-build event such as:

```DOS
copy $(OutDir)\*.* ..\PcfJobHostApp\Bin\$(ConfigurationName)\netcoreapp2.2\Jobs\
```

## Job configuration ##

In addition to the DLLs and supporting files you must create a json file to describe your job.

* Files must be named `*-job.json` and be de-serialiable to `Models.JobInfo` where * can be any valid file-system characters.

> Note: Linux is case sensitive. Also we suggest limiting your names to include only `[A-Za-z0-9_-]` with no white-space.

> Notice that the `json` file is set to `Content` and `Copy Always`


### Field Information ###

* namespace is the dll name (and namespace)
* schedule is the Quarz Version of CRON https://www.freeformatter.com/cron-expression-generator-quartz.html
* parameters is a 0:many array of Name/Value pairs (optional)

### Sample JSON ###

```JSON
{
  "NameSpace": "ExampleJob",
  "Schedule": "0/30 * * * * ?",
  "Parameters": [
    {
      "Name": "Name3",
      "Value": "Value3"
    },
    {
      "Name": "Name4",
      "Value": "Value4"
    }
  ]
}
```

## Customizing the Job Host ##

> See `TODO:` items in the scheduler for areas teams may want to customize.

## License
Copyright (c) 2018-2019
Licensed under the [MIT license](LICENSE).

## About me ##

**Stuart Williams**

* I Cloud. I Code. 
* <a href="mailto:stuart.t.williams@outlook.com" target="_blank">stuart.t.williams@outlook.com</a> (e-mail)
* Blog: <a href="http://blitzkriegsoftware.net/Blog" target="_blank">http://blitzkriegsoftware.net/Blog</a>
* LinkedIn: <a href="http://lnkd.in/P35kVT" target="_blank">http://lnkd.in/P35kVT</a>
* YouTube: <a href="https://www.youtube.com/user/spookdejur1962/videos" target="_blank">https://www.youtube.com/user/spookdejur1962/videos</a> 